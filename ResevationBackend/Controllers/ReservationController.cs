using Microsoft.AspNetCore.Mvc;
using ResevationBackend.Services;

namespace ResevationBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost("{appointmentSlotId}/reserveAppointment")]
        public async Task<IActionResult> ReserveSlot(int appointmentSlotId, [FromQuery] int clientId)
        {
            try 
            { 
                var result = await _reservationService.ReserveSlotAsync(clientId, appointmentSlotId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("{reservationId}/ConfirmReservation")]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            try
            {
                await _reservationService.ConfirmReservationAsync(reservationId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("availableSlots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] int? providerId, [FromQuery] DateTime? startDate) 
        {
            //would replace this with a background job
            await _reservationService.CancelExpiredReservationsAsync();
            try
            {
                var slots = await _reservationService.GetAvailableSlotsAsync(providerId, startDate);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
