using CalendarAssistant.Models;
using CalendarAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarAssistant.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("GetScheduleByUserId")]
        public async Task<ActionResult<IEnumerable<ScheduleModel>>> GetScheduleByUserId(int userId = 1)
        {
            try
            {
                var scheduleList = await _scheduleService.GetWeeklySchedule(userId);
                return Ok(scheduleList);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching user's weekly schedule: {ex.Message}");
            }
        }

        [HttpPost("SaveSchedule")]
        public async Task<ActionResult<bool>> SaveSchedule(List<ScheduleModel> scheduleModel)
        {
            try
            {
                bool result = await _scheduleService.SaveWeeklySchedule(scheduleModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching user's weekly schedule: {ex.Message}");
            }
        }

        [HttpGet("GetAllTimeZone")]
        public async Task<ActionResult<IEnumerable<Models.TimeZone>>> GetAllTimeZone()
        {
            try
            {
                var timeZoneList = await _scheduleService.GetAllTimeZone();
                return Ok(timeZoneList);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching time zone list: {ex.Message}");
            }
        }

        [HttpGet("GetUserTimeZoneMapping")]
        public async Task<ActionResult<Models.TimeZone>> GetUserTimeZoneMapping(int userId= 1)
        {
            try
            {
                var result = await _scheduleService.GetUserTimeZoneMapping(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching time zone list: {ex.Message}");
            }
        }

        [HttpPost("SaveUserTimeZoneMapping")]
        public async Task<ActionResult<bool>> SaveUserTimeZoneMapping(UserTimeZoneMappingSaveModel userTimeZoneMappingSaveModel)
        {
            try
            {
                bool result = await _scheduleService.SaveUserTimeZoneMapping(userTimeZoneMappingSaveModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching user's weekly schedule: {ex.Message}");
            }
        }
    }
}
