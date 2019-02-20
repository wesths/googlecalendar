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

        public List<Appointment> GetAppointments(DateTime startDate, DateTime endDate)
        {
            List<Appointment> appointments = new List<Appointment>();

            int diffMinutes = (int)endDate.Subtract(startDate).TotalMinutes;
            //int hours = (int)End.Subtract(Start).TotalHours;
            int intervalMinutes = _configService.GetIntervalMinutes();
            int runningTotalMinutes = 0;
            int id = 1;

            while(runningTotalMinutes < diffMinutes)
            {
                var newStart = startDate.AddMinutes(runningTotalMinutes);
                runningTotalMinutes += intervalMinutes;

                var appointment = new Appointment()
                {
                    Id = id,
                    IsBusy = false,
                    IntervalMinutes = intervalMinutes,
                    StartDate = newStart,
                    TimeOfDayCategory = GetTimeOfDay(newStart)
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

        public List<Appointment> FreeBusyCheck(DateTime startDate, DateTime endDate)
        { 
            FreeBusyRequest request = new FreeBusyRequest();
            request.TimeMin = startDate;
            request.TimeMax = endDate;
            var items = new List<FreeBusyRequestItem>();
            var item = new FreeBusyRequestItem();
            item.Id = calendarId;
            item.ETag = "";
            items.Add(item);
            request.Items = items;
            var appointments = GetAppointments(startDate, endDate);

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
                        }
                    }
                }
            }
            return appointments;
        }

        public List<string> GetDatesAvaliable(int productDurationMinutes)
        {
            //DateTime startDate = DateTime.Today;
            //DateTime endDate = new DateTime(2019, 03, 01);

            List<string> days = new List<string>();

            //TODAY
            DateTime today = DateTime.Today;
            DateTime startDate = new DateTime(today.Year, today.Month, today.Day, 08, 00, 00);
            DateTime endDate = new DateTime(today.Year, today.Month, today.Day, 20, 00, 00);

            if (GetAvailableTimeSlots(productDurationMinutes, startDate, endDate))
            {
                days.Add("TODAY");
            }

            // TOMORROW
            DateTime tomorrow = DateTime.Today.AddDays(1);
            DateTime startDateTomorrow = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 08, 00, 00);
            DateTime endDateTomorrow = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 20, 00, 00);

            if (GetAvailableTimeSlots(productDurationMinutes, startDateTomorrow, endDateTomorrow))
            {
                days.Add("TOMORROW");
            }

            // TODAY - TOMORROW - THIS WEEK - NEXT WEEK

            // THIS WEEK
            // = MON - TUE - WED - THU - FRI - SAT - SUN

            // NEXT WEEK
            // = 25 - 26 - 27 - 28 - 01 - 02

            return days;
        }

        public List<string> GetThisWeekAvailable(int productDurationMinutes)
        {
            List<string> daysThisWeekAvailable = new List<string>();

            DateTime today = DateTime.Now;
            today = today.AddDays(2); // Today and tomorrow catered for already
            DayOfWeek day = today.DayOfWeek;
            int days = day - DayOfWeek.Monday;
            DateTime startDate = today;
            DateTime endDate = startDate.AddDays(6-days);

            var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(x => startDate.AddDays(x)).ToList();

            foreach(var date in dates)
            {
                var startDateTime = new DateTime(date.Year, date.Month, date.Day, 08, 00, 00);
                var endDateTime = new DateTime(date.Year, date.Month, date.Day, 20, 00, 00);

                if (GetAvailableTimeSlots(productDurationMinutes, startDateTime, endDateTime))
                {
                    daysThisWeekAvailable.Add($"{date.DayOfWeek.ToString()}");
                }
            }
            return daysThisWeekAvailable;
        }

        private bool GetAvailableTimeSlots(int productDurationMinutes, DateTime startDate, DateTime endDate)
        {
            List<string> busy = new List<string>();

            var appointments = FreeBusyCheck(startDate, endDate);

            foreach (var appointment in appointments)
            {
                // we work in 15 minute intervals. Logic requires to subtract 15 min from desired appointment duration
                var checkStartTime = appointment.StartDate.AddMinutes(productDurationMinutes - 15);

                // if there is not enought time to create an appointment mark as busy
                if (appointments.Where(x => x.StartDate <= checkStartTime && x.StartDate >= appointment.StartDate && x.IsBusy == true).Any())
                {
                    appointments[appointments.IndexOf(appointment)].IsBusy = true;
                }
                // within working hours
                if (checkStartTime >= endDate)
                {
                    appointments[appointments.IndexOf(appointment)].IsBusy = true;
                }
            }

            var availableToBook = appointments.Where(x => x.IsBusy == false)
            //var group = appointments.Where(x => x.IsBusy == false)
                .Any();

            return availableToBook;

        }


        public List<string> GetAvailableTimeSlots(TimeOfDay timeOfDay, int productDurationMinutes)
        {
            DateTime startDate = new DateTime(2019, 02, 12, 08, 00, 00);
            DateTime endDate = new DateTime(2019, 02, 12, 20, 00, 00);
            List<string> busy = new List<string>();

            var appointments = FreeBusyCheck(startDate,endDate);

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

            var group = appointments.Where(x => x.IsBusy == false && x.TimeOfDayCategory == timeOfDay)
            //var group = appointments.Where(x => x.IsBusy == false)
                .Select(x => new { x.StartDate, x.Id })
                .OrderBy(x => x.Id)
                .ToList();

            busy = group.Select(x => x.StartDate.ToShortTimeString()).ToList();

            return busy;
            
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
