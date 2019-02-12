using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar
{
    public class Appointment
    {
        public int Id { get; set; }
        public bool IsBusy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IntervalMinutes { get; set; }
    }
}
