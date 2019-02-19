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
        [Route("GetDates")]
        public IActionResult GetDates()
        {

            return Ok(_appointmentService.GetAppointments(15));            

        }

        [HttpGet]
        [Route("BusyCheck")]
        public IActionResult BusyCheck()
        {

            return Ok(_appointmentService.FreeBusyCheck());

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
    }
}