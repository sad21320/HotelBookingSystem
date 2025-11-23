using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class RoomType
    {
        [Key]
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; } // Стандарт, Люкс
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal BasePrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}