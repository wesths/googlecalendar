using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Models
{
    public class AddEventRequest
    {
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Cell { get; set; }
        public string Email { get; set; }
        public int EventDurationMinutes { get; set; }
    }
}
