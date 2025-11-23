using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using HotelBookingSystem.Data;

namespace HotelBookingSystem.Middleware
{
    public class DbInitializerMiddleware
    {
        private readonly RequestDelegate _next;

        public DbInitializerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, HotelDbContext dbContext)
        {
            // Используем сессию, чтобы не дергать базу при каждом запросе страницы
            if (!context.Session.Keys.Contains("db_initialized"))
            {
                DbInitializer.Initialize(dbContext);
                context.Session.SetString("db_initialized", "true");
            }

            await _next.Invoke(context);
        }
    }

    public static class DbInitializerExtensions
    {
        public static IApplicationBuilder UseDbInitializer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DbInitializerMiddleware>();
        }
    }
}