using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GoogleCalendar.Configurations.Contracts;
using GoogleCalendar.Interfaces;
using GoogleCalendar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ICredentialService _credentialService;
        private readonly IConfigService _configService;
        private ServiceAccountCredential credentials;
        private CalendarService service;
        private string calendarId;

        public AppointmentService(ICredentialService credentialService, IConfigService configService)
        {
            _credentialService = credentialService;
            _configService = configService;
            credentials = credentialService.GetServiceAccountCredentials();
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Google Calendar API",

            });
            calendarId = _configService.GetCalenderId();
        }

        public List<DateEntity> GetAvailableDateSlots()
        {
            List<DateEntity> dates = new List<DateEntity>();
            dates.Add(DateEntity.TODAY);
            dates.Add(DateEntity.TOMORROW);
            dates.Add(DateEntity.THISWEEK);
            dates.Add(DateEntity.NEXTWEEK);
            dates.Add(DateEntity.INTWOWEEKS);

            return dates;
        }

        public List<Appointment> GetAppointments(int intervalMinutes)
        {
            List<Appointment> appointments = new List<Appointment>();
            DateTime Start = new DateTime(2019, 02, 12, 8, 0,0);
            DateTime End = new DateTime(2019, 02, 12, 20, 0, 0);
            int diffMinutes = (int)End.Subtract(Start).TotalMinutes;
            //int hours = (int)End.Subtract(Start).TotalHours;
            int min15 = 0;
            int id = 1;

            while(min15 < diffMinutes)
            {
                var newStart = Start.AddMinutes(min15);
                min15 += 15;
               // var newEnd = newStart.AddMinutes(min15);

                var appointment = new Appointment()
                {
                    Id = id,
                    IsBusy = false,
                    IntervalMinutes = intervalMinutes,
                    StartDate = newStart,
                    TimeOfDayCategory = GetTimeOfDay(newStart)
           //         EndDate = newEnd
                };
                appointments.Add(appointment);
                id++;
                
            }


            return appointments;
        }

        private TimeOfDay GetTimeOfDay(DateTime date)
        {
            TimeOfDay tod = TimeOfDay.MORNING;

            if (date.Hour > 16)
                tod = TimeOfDay.EVENING;
            else if (date.Hour > 11)
                tod = TimeOfDay.AFTERNOON;
            else
                tod = TimeOfDay.MORNING;

            return tod;
        }

        public List<Appointment> FreeBusyCheck()
        {
            DateTime startDate = new DateTime(2019, 02, 12);
            DateTime endDate = new DateTime(2019, 02, 13);
 
            FreeBusyRequest request = new FreeBusyRequest();
            request.TimeMin = startDate;
            request.TimeMax = endDate;
            var items = new List<FreeBusyRequestItem>();
            var item = new FreeBusyRequestItem();
            item.Id = calendarId;
            item.ETag = "";
            items.Add(item);
            request.Items = items;
            var appointments = GetAppointments(15);

            FreebusyResource.QueryRequest testRequest = service.Freebusy.Query(request);
            var responseObject = testRequest.Execute();

            if (responseObject.Calendars.ContainsKey(calendarId))
            {
                if (responseObject.Calendars[calendarId].Busy.Count > 0)
                {
                    foreach (var busyItem in responseObject.Calendars[calendarId].Busy)
                    {

                        var busyStart = busyItem.Start;
                        var busyEnd = busyItem.End;

                        // gets all the available date times between busy start and end date
                        var starts = appointments.Where(a => a.StartDate >= busyStart).ToList();
                        var ends = starts.Where(a => a.StartDate < busyEnd).ToList();

                        foreach (var appointment in ends)
                        {
                            appointments[appointments.IndexOf(appointment)].IsBusy = true;
                            //appointments = appointments.Where(a => a.Id == appointment.Id).Select(u => { u.IsBusy = true; return u; }).ToList();

                        }
                    }

                }
            }
            return appointments;
        }



        public List<string> GetAvailableTimeSlots(TimeOfDay timeOfDay, int productDurationMinutes)
        {
            List<string> busy = new List<string>();


            var appointments = FreeBusyCheck();

            foreach (var appointment in appointments)
            {
                // we work in 15 minute intervals. Logic requires to subtract 15 min from desired appointment duration
                var checkStartTime = appointment.StartDate.AddMinutes(productDurationMinutes - 15);

                // if there is not enought time to create an appointment mark as busy
                if (appointments.Where(x => x.StartDate <= checkStartTime && x.StartDate >= appointment.StartDate && x.IsBusy == true).Any())
                {
                    appointments[appointments.IndexOf(appointment)].IsBusy = true;
                }
            }

            //var newGroup = appointments.Where(x => x.StartDate.TimeOfDay.Add(TimeSpan.FromMinutes(productDurationMinutes)) != x.IsBusy  && x.TimeOfDayCategory == timeOfDay)
            //    .Select(x => new { x.StartDate, x.Id })
            //    .OrderBy(x => x.Id)
            //    .ToList();

            //var group = appointments.Where(x => x.IsBusy == false && x.TimeOfDayCategory == timeOfDay)
            var group = appointments.Where(x => x.IsBusy == false)
                .Select(x => new { x.StartDate, x.Id })
                .OrderBy(x => x.Id)
                .ToList();

            busy = group.Select(x => x.StartDate.ToShortTimeString()).ToList();

            return busy;

            //string calenderId = _configService.GetCalenderId();
            //EventsResource.ListRequest request = service.Events.List(calenderId);
            //request.TimeMin = new DateTime(2019,02,12);
            //request.TimeMax = new DateTime(2019, 02, 13);
            //request.ShowDeleted = false;
            //request.SingleEvents = true;
            ////request.MaxResults = 10;
            //request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            //// List events.
            //Events events = request.Execute();
            
            //if (events.Items != null && events.Items.Count > 0)
            //{
            //    foreach (var eventItem in events.Items)
            //    {
            //        string when = eventItem.Start.DateTime.ToString();
            //        if (String.IsNullOrEmpty(when))
            //        {
            //            when = eventItem.Start.Date;
            //        }
            //        busy.Add(eventItem.Summary + " - " + when);
            //    }
            //}
            //else
            //{
            //    busy.Add("No upcoming events found.");
            //}
            //return busy;
            
        }

        public string AddAppointment(AddEventRequest request)
        {
            string hours = request.EventTime.Substring(0, 2);

            DateTime startDate = new DateTime(request.EventDate.Year, request.EventDate.Month, request.EventDate.Day, Convert.ToInt32(request.EventTime.Substring(0, 2)), Convert.ToInt32(request.EventTime.Substring(3, 2)), 0);
            DateTime endDate = startDate.AddMinutes(request.EventDurationMinutes);
            Event appointment = new Event
            {
                Summary = "Hair Appointment",
                Start = new EventDateTime()
                {
                    DateTime = startDate,
                    TimeZone = "Africa/Johannesburg"
                },
                End = new EventDateTime()
                {
                    DateTime = endDate,
                    TimeZone = "Africa/Johannesburg"
                },
                Description = $"Customer: {request.FirstName} {request.LastName} with Cell Number: {request.Cell} and Email: {request.Email}"
            };

            var newEventRequest = service.Events.Insert(appointment, calendarId);
            newEventRequest.SendNotifications = true;
            var eventResult = newEventRequest.Execute();

            return eventResult.Id;

        }
    }
    
}
