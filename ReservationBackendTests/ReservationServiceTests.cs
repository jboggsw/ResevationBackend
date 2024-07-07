using Microsoft.EntityFrameworkCore;
using ResevationBackend.Database;
using ResevationBackend.Models;
using ResevationBackend.Services;

namespace ReservationBackendTests
{
    public class ReservationServiceTests
    {
        private readonly ReservationService _reservationService;
        private readonly ReservationBackendContext _context;

        public ReservationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ReservationBackendContext>()
                .UseInMemoryDatabase("ReservationDatabase")
                .Options;
            _context = new ReservationBackendContext(options);
            _reservationService = new ReservationService(_context);

        }



        [Test]
        public async Task ShouldReturnAvailableSlots()
        {
            var provider = new Provider
            {
                Name = "Dr. Jekyll",
            };
            _context.Providers.Add(provider);
 
            var slot1 = new AppointmentSlot { StartTime = DateTime.Today.AddHours(9), EndTime = DateTime.Today.AddHours(9).AddMinutes(15), IsReserved = false, ProviderId = provider.Id };
            var slot2 = new AppointmentSlot { StartTime = DateTime.Today.AddHours(10), EndTime = DateTime.Today.AddHours(10).AddMinutes(15), IsReserved = false, ProviderId = provider.Id };
            _context.AppointmentSlots.Add(slot1);
            _context.AppointmentSlots.Add(slot2);
                    
            await _context.SaveChangesAsync();

            var slots = await _reservationService.GetAvailableSlotsAsync(provider.Id, DateTime.Today);

            Assert.That(slots.Count() == 2);
        }

        [Test]
        public async Task shouldReserveSlot()
        {
           
            var client = new Client { Name = "Test Client" };
            var slot = new AppointmentSlot { StartTime = DateTime.Today.AddHours(48), EndTime = DateTime.Today.AddHours(48).AddMinutes(15), IsReserved = false };
            _context.Clients.Add(client);
            _context.AppointmentSlots.Add(slot);
            await _context.SaveChangesAsync();

            var reservation = await _reservationService.ReserveSlotAsync(client.Id, slot.Id);

            Assert.That(reservation != null);
            Assert.That(_context.AppointmentSlots.FirstOrDefault(s => s.Id == slot.Id).IsReserved);
        }

        [Test]
        public async Task ConfirmReservation_ShouldConfirmReservation()
        {
            var reservation = new Reservation { ClientId = 1, AppointmentSlotId = 1, IsConfirmed = false, ReservationTime = DateTime.UtcNow };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await _reservationService.ConfirmReservationAsync(reservation.Id);

            var confirmedReservation = _context.Reservations.FirstOrDefault(r => r.Id == reservation.Id);
            Assert.That(confirmedReservation.IsConfirmed);
        }

        [Test]
        public async Task CancelExpiredReservations_ShouldCancelExpiredReservations()
        {
            var slot = new AppointmentSlot { StartTime = DateTime.Today.AddHours(10), EndTime = DateTime.Today.AddHours(10).AddMinutes(15), IsReserved = true };
            _context.AppointmentSlots.Add(slot);
            await _context.SaveChangesAsync();

            var reservation = new Reservation { ClientId = 1, AppointmentSlotId = slot.Id, IsConfirmed = false, ReservationTime = DateTime.UtcNow.AddMinutes(-31) };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await _reservationService.CancelExpiredReservationsAsync();

            var expiredReservation = _context.Reservations.FirstOrDefault(r => r.Id == reservation.Id);
            Assert.Null(expiredReservation);
            var updatedSlot = _context.AppointmentSlots.FirstOrDefault(s => s.Id == slot.Id);
            Assert.False(updatedSlot.IsReserved);
        }


        [OneTimeTearDown]
        public void TearDown() => _context.Dispose();


    }
}