using HotelBookingSystem.Models;
using System.Linq;

namespace HotelBookingSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HotelDbContext context)
        {
            // Создает структуру БД, если её нет
            context.Database.EnsureCreated();

            // ПРОВЕРКА: Если есть хоть одна запись, выходим. 
            // Это гарантирует, что данные берутся из БД, а не перезаписываются.
            if (context.Guests.Any())
            {
                return;
            }

            // --- Если база абсолютно пустая, добавляем тестовые данные (один раз) ---
            // Сюда можно вставить код начального заполнения, если нужно, 
            // или оставить пустым, если ты заливаешь данные через SQL скрипт.

            // Пример заполнения (если нужно):
            /*
            for (int i = 1; i <= 25; i++)
            {
                context.RoomTypes.Add(new RoomType { TypeName = $"Type {i}", BasePrice = 100 * i, Capacity = 2 });
            }
            context.SaveChanges();
            // ... остальные таблицы
            */
        }
    }
}