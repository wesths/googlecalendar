using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleCalendar.Interfaces;
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

            return Ok(_appointmentService.GetAvailableTimeSlots());            

        }

        [HttpGet]
        [Route("BusyCheck")]
        public IActionResult BusyCheck()
        {

            return Ok(_appointmentService.FreeBusyCheck());

        }
    }
}