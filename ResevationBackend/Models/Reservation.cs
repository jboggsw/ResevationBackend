namespace ResevationBackend.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int AppointmentSlotId { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime ReservationTime { get; set; }
    }
}
