using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class AdditionalService
    {
        [Key]
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}