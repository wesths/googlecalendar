using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GoogleCalendar.Configurations.Contracts;
using GoogleCalendar.Interfaces;
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
            DateTime End = new DateTime(2019, 02, 12, 17, 0, 0);
            int hours = (int)End.Subtract(Start).TotalHours;

            for (int h = 1; h < hours; h++)
            {
                var newStart = Start.AddHours(h);
                var newEnd = newStart.AddMinutes(intervalMinutes);

                var appointment = new Appointment()
                {
                    Id = h,
                    IsBusy = false,
                    IntervalMinutes = intervalMinutes,
                    StartDate = newStart,
                    EndDate = newEnd
                };
                appointments.Add(appointment);
            }

            return appointments;
        }

        public List<string> FreeBusyCheck()
        {
            DateTime startDate = new DateTime(2019, 02, 12);
            DateTime endDate = new DateTime(2019, 02, 13);
            List<string> busy = new List<string>();
            FreeBusyRequest request = new FreeBusyRequest();
            request.TimeMin = startDate;
            request.TimeMax = endDate;
            var items = new List<FreeBusyRequestItem>();
            var item = new FreeBusyRequestItem();
            item.Id = calendarId;
            item.ETag = "";
            items.Add(item);
            request.Items = items;
            // request.Items.Add(item);
            var appointments = GetAppointments(60);



            FreebusyResource.QueryRequest testRequest = service.Freebusy.Query(request);
            var responseObject = testRequest.Execute();

            bool checkBusy;
            bool containsKey;
            if (responseObject.Calendars.ContainsKey(calendarId))
            {
                containsKey = true;
                if (containsKey)
                {
                    //Had to deconstruct API response by WriteLine(). Busy returns a count of 1, while being free returns a count of 0. 
                    //These are properties of a dictionary and a List of the responseObject (dictionary returned by API POST).
                    if (responseObject.Calendars[calendarId].Busy.Count == 0)
                    {
                        checkBusy = false;
                        //WriteLine(checkBusy);
                    }
                    else
                    {
                        checkBusy = true;
                        //WriteLine(checkBusy);
                    }

                    if (checkBusy == true)
                    {
                        foreach(var busyItem in responseObject.Calendars[calendarId].Busy)
                        {

                            var busyStart = busyItem.Start;
                            var busyEnd = busyItem.End;

                            //var appointment = appointments.Where(a => busyStart >= a.StartDate && busyEnd <= a.EndDate).ToList();
                            //if (appointment.Count == 0 )
                            //{
                                var starts = appointments.Where(a => a.StartDate >= busyStart).ToList();
                                var ends = starts.Where(a => a.EndDate <= busyEnd).ToList();
                                var end2 = starts.Where(a => busyEnd >= a.StartDate && busyEnd <= a.EndDate).ToList();

                                var finalBooking = appointments.Where(a => a.Id >= starts.Select(x => x.Id).FirstOrDefault() && a.Id <= ends.Select(x => x.Id).LastOrDefault()).ToList();
                            //}

                            
                            busy.Add("Between " + busyStart + " and " + busyEnd + " your trainer is busy");

                        }                 

                        
                        
                    }
                    else
                    {
                        busy.Add("Between " + startDate + " and " + endDate + " your trainer is free");
                        
                    }
                }                
            }
            return busy;
        }

        public List<string> GetAvailableTimeSlots()
        {
            List<string> busy = new List<string>();

            string calenderId = _configService.GetCalenderId();
            EventsResource.ListRequest request = service.Events.List(calenderId);
            request.TimeMin = new DateTime(2019,02,12);
            request.TimeMax = new DateTime(2019, 02, 13);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            //request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    busy.Add(eventItem.Summary + " - " + when);
                }
            }
            else
            {
                busy.Add("No upcoming events found.");
            }
            return busy;
            
        }
    }
    
}
