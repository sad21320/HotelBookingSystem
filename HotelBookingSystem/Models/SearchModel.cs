namespace HotelBookingSystem.Models
{
    public class SearchModel
    {
        public string GuestName { get; set; } = "";
        public string SearchType { get; set; } = "Room";
        public string FilterMode { get; set; } = "Active";
    }
}