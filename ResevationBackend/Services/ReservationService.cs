using Microsoft.EntityFrameworkCore;
using ResevationBackend.Database;
using ResevationBackend.Models;

namespace ResevationBackend.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableSlotsAsync(int? providerId, DateTime? date);
        Task<Reservation> ReserveSlotAsync(int clientId, int slotId);
        Task ConfirmReservationAsync(int reservationId);
        Task CancelExpiredReservationsAsync();
        Task SetAvailabilityAsync(int providerId, List<DataTimeRange> dateTimeRanges);
        Task<IEnumerable<Provider>> GetProvidersAsync();
    }
    //needs more robust error handling and validation. Would need to address before going to production
    //would also need to implement logging and monitoring
    //need to implement transactions
    public class ReservationService : IReservationService
    {
        private readonly ReservationBackendContext _context;

        public ReservationService(ReservationBackendContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableSlotsAsync(int? providerId, DateTime? date)
        {
            IQueryable<AppointmentSlot> query = _context.AppointmentSlots.Where(s => !s.IsReserved);

            if (date.HasValue)
            {
                query = query.Where(s => s.StartTime.Date == date.Value.Date);
            }

            if (providerId.HasValue)
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == providerId.Value);

                if (provider == null)
                {
                    throw new Exception("Provider not found");
                }

                query = query.Where(s => s.ProviderId == providerId.Value);


            }

            var slots = await query.ToListAsync();

            return slots;
        }

        public async Task<Reservation> ReserveSlotAsync(int clientId, int slotId)
        {
            var slot = await _context.AppointmentSlots.FirstOrDefaultAsync(s => s.Id == slotId && !s.IsReserved);
            if (slot == null)
            {
                throw new Exception("Appointment slot not found");
            }


            var reservation = new Reservation
            {
                ClientId = clientId,
                AppointmentSlotId = slotId,
                IsConfirmed = false,
                ReservationTime = DateTime.UtcNow
            };
           
            if (slot.StartTime < DateTime.UtcNow.AddHours(24))
            {
                throw new Exception("Reservations must be made at least 24 hours in advance");
            }





            slot.IsReserved = true;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);
            if (reservation == null)
            {
                throw new Exception("Reservation not found");
            }

            reservation.IsConfirmed = true;
            await _context.SaveChangesAsync();
        }

        public async Task CancelExpiredReservationsAsync()
        {
            var expiredReservations = _context.Reservations
                .Where(r => !r.IsConfirmed && r.ReservationTime < DateTime.UtcNow.AddMinutes(-30))
                .ToList();

            foreach (var reservation in expiredReservations)
            {
                var slot = await _context.AppointmentSlots.FirstOrDefaultAsync(s => s.Id == reservation.AppointmentSlotId);
                if (slot != null)
                {
                    slot.IsReserved = false;
                }
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
        }
        public async Task SetAvailabilityAsync(int providerId, List<DataTimeRange> dateTimeRanges)
        {
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider == null)
            {
                throw new Exception("Provider not found");
            }

            foreach (var range in dateTimeRanges)
            {
                var start = range.StartTime;
                var end = range.EndTime;

                while (start < end)
                {
                    var slot = new AppointmentSlot
                    {
                        ProviderId = providerId,
                        StartTime = start,
                        EndTime = start.AddMinutes(15),
                        IsReserved = false
                    };

                    _context.AppointmentSlots.Add(slot);
                    start = start.AddMinutes(15);
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Provider>> GetProvidersAsync()
        {
            return await _context.Providers.ToListAsync();
        }

    }

}
