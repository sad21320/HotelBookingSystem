using System.Collections.Generic;

namespace HotelBookingSystem.Services
{
    // Универсальный интерфейс, как в отчете
    public interface ICachedService<T> where T : class
    {
        void AddEntities(string cacheKey, int rowsNumber = 20);
        IEnumerable<T> GetEntities(string cacheKey, int rowsNumber = 20);
    }
}