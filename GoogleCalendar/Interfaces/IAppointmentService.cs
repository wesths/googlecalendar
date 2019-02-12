using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Interfaces
{
    public interface IAppointmentService
    {
        List<DateEntity>GetAvailableDateSlots();
        List<string> GetAvailableTimeSlots();
        List<string> FreeBusyCheck();

        List<Appointment> GetAppointments(int intervalMinutes);
    }
}
