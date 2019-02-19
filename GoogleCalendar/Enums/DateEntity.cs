using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar
{
    public enum DateEntity
    {
        TODAY,
        TOMORROW,
        THISWEEK,
        NEXTWEEK,
        INTWOWEEKS
    }

    public enum TimeOfDay
    {
        MORNING,
        AFTERNOON,
        EVENING
    }
}
