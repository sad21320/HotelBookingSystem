using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class Guest
    {
        [Key]
        public int GuestID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}