using GoogleCalendar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleCalendar.Interfaces
{
    public interface IAppointmentService
    {
        List<DateEntity>GetAvailableDateSlots();
        List<string> GetAvailableTimeSlots(TimeOfDay timeOfDay, int productDurationMinutes);
        List<Appointment> FreeBusyCheck();
        List<Appointment> GetAppointments(int intervalMinutes);
        string AddAppointment(AddEventRequest request);
    }
}
