using Microsoft.AspNetCore.Mvc;
using ResevationBackend.Models;
using ResevationBackend.Services;

namespace ResevationBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvidersController : ControllerBase
    { 
        private readonly IReservationService _reservationService;

        public ProvidersController(IReservationService reservationService)
        {
            _reservationService = reservationService;    
        }
        //would need to implement some form of authentication and authorization
        [HttpPost("{providerId}/setAvailability")]
        public async Task<IActionResult> SetAvailability(int providerId, [FromBody] List<DataTimeRange> dateTimeRanges)
        {
            try
            {
                await _reservationService.SetAvailabilityAsync(providerId, dateTimeRanges);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetProviders()
        {

            var providers = await _reservationService.GetProvidersAsync();
            return Ok(providers);
        }

    }
}
