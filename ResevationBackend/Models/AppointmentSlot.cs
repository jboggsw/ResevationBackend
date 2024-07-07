namespace ResevationBackend.Models
{
    public class AppointmentSlot
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsReserved { get; set; }
        public int ProviderId { get; set; }
    }
}
