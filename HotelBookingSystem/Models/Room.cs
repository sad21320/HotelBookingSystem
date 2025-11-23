using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Models
{
    public class Room
    {
        [Key]
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public int RoomTypeID { get; set; } // Внешний ключ
        public int Floor { get; set; }
        public string Status { get; set; } // Свободен, Занят
        public DateTime CreatedDate { get; set; }

        // Навигационное свойство (чтобы узнать цену и название типа)
        public RoomType RoomType { get; set; }
    }
}