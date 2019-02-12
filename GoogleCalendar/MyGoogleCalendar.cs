using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleCalendar
{
    public class MyGoogleCalendar
    {
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";
        CalendarService service;

        public MyGoogleCalendar()
        {
            //UserCredential credential;

            //using (var stream =
            //    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            //{
            //    // The file token.json stores the user's access and refresh tokens, and is created
            //    // automatically when the authorization flow completes for the first time.
            //    string credPath = "token.json";
            //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        GoogleClientSecrets.Load(stream).Secrets,
            //        Scopes,
            //        "user",
            //        CancellationToken.None).Result;
            //     //   new FileDataStore(credPath, true)).Result;
            //   // Console.WriteLine("Credential file saved to: " + credPath);
            //}

            String serviceAccountEmail = "calendar@quickstart-1549630559785.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { CalendarService.Scope.CalendarReadonly }
               }.FromCertificate(certificate));


            // Create Google Calendar API service.
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
                
            });

        }
        // Today
        // Tomorrow
        // This Week
        // Next Week
        // In 2 Weeks
        

        public string GetEvents()
        {
            string result = "";
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("msqrl4enca37ph7qvkf06r5cq0@group.calendar.google.com");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                    result += eventItem.Summary + "- DateTime: " + when;
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            return result;
        }
    }
}
