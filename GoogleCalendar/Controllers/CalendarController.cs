using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleCalendar.Interfaces;
using GoogleCalendar.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoogleCalendar.Controllers
{
    [Produces("application/json")]
    [Route("api/Calendar")]
    public class CalendarController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public CalendarController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }


        [HttpGet]
        [Route("BusyCheck")]
        public IActionResult BusyCheck()
        {
            DateTime startDate = new DateTime(2019, 02, 12, 08, 00, 00);
            DateTime endDate = new DateTime(2019, 02, 12, 20, 00, 00);
            return Ok(_appointmentService.FreeBusyCheck(startDate, endDate));

        }

        [HttpGet]
        [Route("GetAvailableTimeSlots")]
        public IActionResult GetAvailableTimeSlots(int tod, int productMinutes)
        {

            return Ok(_appointmentService.GetAvailableTimeSlots((TimeOfDay)tod, productMinutes));

        }

        [HttpPost]
        [Route("AddEvent")]
        public IActionResult AddEvent([FromBody]AddEventRequest request)
        {

            return Ok(_appointmentService.AddAppointment(request));

        }

        [HttpGet]
        [Route("GetDatesAvailable")]
        public IActionResult GetDatesAvailable(int productMinutes)
        {

            return Ok(_appointmentService.GetDatesAvaliable(productMinutes));

        }

        [HttpGet]
        [Route("TestDayOfWeek")]
        public IActionResult TestDayOfWeek(int productMinutes)
        {
            
            return Ok(_appointmentService.GetThisWeekAvailable(productMinutes));

        }
    }
}