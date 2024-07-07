using Microsoft.EntityFrameworkCore;
using ResevationBackend.Models;

namespace ResevationBackend.Database

{
    public class ReservationBackendContext : DbContext
    {
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<AppointmentSlot> AppointmentSlots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public ReservationBackendContext(DbContextOptions<ReservationBackendContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //just using in memory database for testing
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseInMemoryDatabase("ReservationDatabase");


        }
    }
}

