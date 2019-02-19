using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public bool IsBusy { get; set; }
        public DateTime StartDate { get; set; }
      //  public DateTime EndDate { get; set; }
        public int IntervalMinutes { get; set; }
        public TimeOfDay TimeOfDayCategory { get; set; }
        // Morning
        // -- > 12:00AM and < 12:00PM
        // Afternoon
        // -- > 12:00PM and < 05:00PM
        // Evening
        // -- > 05:00PM and < 12:00AM
    }
}
