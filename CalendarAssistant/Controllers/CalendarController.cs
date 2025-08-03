using CalendarAssistant.ActionFilters;
using CalendarAssistant.Models;
using CalendarAssistant.Services;
using Google.Apis.Auth;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;

namespace CalendarAssistant.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ILogger<CalendarController> _logger;

        private readonly IGoogleCalendarService _googleCalendarService;

        public CalendarController(ILogger<CalendarController> logger, IGoogleCalendarService googleCalendarService)
        {
            _logger = logger;
            _googleCalendarService = googleCalendarService;
        }


        [HttpPost("AuthenticateUser")]
        public async Task<ActionResult> AuthenticateUser([FromBody] Models.TokenRequest request)
        {
            try
            {
                var googleAuthentication = await _googleCalendarService.AuthenticateUser(request);
                return Ok(googleAuthentication);
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid Google token");
            }
        }

        [TokenValidationFilter]
        [HttpGet("GetAllEvents")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetAllEvents()
        {
            try
            {
                var accessToken = Request.Headers["Access_token"].FirstOrDefault();
                var calendarEvents = await _googleCalendarService.GetAllEvents(accessToken);

                return Ok(calendarEvents);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching events: {ex.Message}");
            }
        }

        [TokenValidationFilter]
        [HttpGet("GetAllUserEvents")]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllUserEvents()
        {
            try
            {
                var accessToken = Request.Headers["Access_token"].FirstOrDefault();
                var calendarEvents = await _googleCalendarService.GetAllEvents(accessToken);

                return Ok(calendarEvents);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching events: {ex.Message}");
            }
        }

        [TokenValidationFilter]
        [HttpPost("Schedule")]
        public async Task<ActionResult<Event>> Schedule(CalendarEvent calendarEvent, CancellationToken cancellationToken)
        {
            try
            {
                bool isEventCreated = await _googleCalendarService.Schedule(calendarEvent, cancellationToken);
                if (isEventCreated)
                    return Ok(new Response { Status = "Success", Message = "Event created successfully!" });
                else
                    return BadRequest($"Error creating event");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating event: {ex.Message}");
            }
        }

        [TokenValidationFilter]
        [HttpPost("Cancel")]
        public async Task<ActionResult<Event>> Cancel([FromBody] string eventId, CancellationToken cancellationToken)
        {
            try
            {
                bool isEventCancelled = await _googleCalendarService.Cancel(eventId, cancellationToken);

                if (isEventCancelled)
                    return Ok(new Response { Status = "Success", Message = "Event cancelled successfully!" });
                else
                    return BadRequest($"Error cancelling event");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error cancelling event: {ex.Message}");
            }
        }

        [TokenValidationFilter]
        [HttpPost("Reschedule")]
        public async Task<ActionResult<Event>> Reschedule([FromBody] UpdateEvent updateEvent, CancellationToken cancellationToken)
        {
            try
            {
                var accessToken = Request.Headers["Access_token"].FirstOrDefault();
                var createdEvent = await _googleCalendarService.Reschedule(updateEvent, accessToken);

                return Ok(createdEvent);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error cancelling event: {ex.Message}");
            }
        }

        [TokenValidationFilter]
        [HttpGet("GetEventById")]
        public async Task<ActionResult<Event>> GetEventById(string eventId)
        {
            try
            {
                var accessToken = Request.Headers["Access_token"].FirstOrDefault();
                var eventObj = await _googleCalendarService.GetEventById(eventId, accessToken);

                return Ok(eventObj);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching event: {ex.Message}");
            }
        }

       
    }
}