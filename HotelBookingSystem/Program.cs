using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services;
using HotelBookingSystem.Middleware; // Для DbInitializer
using HotelBookingSystem.Infrastructure; // Для SessionExtensions
using System.Text;
using System.Text.Json; // Для JSON в куки

var builder = WebApplication.CreateBuilder(args);

// --- 1. РЕГИСТРАЦИЯ СЕРВИСОВ ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddMemoryCache();

// Подключение БД
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация Generic сервисов (по одной строке на каждую таблицу)
builder.Services.AddScoped<ICachedService<Guest>, CachedService<Guest>>();
builder.Services.AddScoped<ICachedService<Booking>, CachedService<Booking>>();
builder.Services.AddScoped<ICachedService<Room>, CachedService<Room>>();
builder.Services.AddScoped<ICachedService<RoomType>, CachedService<RoomType>>();
builder.Services.AddScoped<ICachedService<Employee>, CachedService<Employee>>();
builder.Services.AddScoped<ICachedService<AdditionalService>, CachedService<AdditionalService>>();

var app = builder.Build();

// --- 2. НАСТРОЙКА КОНВЕЙЕРА (MIDDLEWARE) ---
app.UseSession();
app.UseDbInitializer(); // Наш инициализатор БД

// Middleware для /info
app.Map("/info", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "text/html;charset=utf-8";
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><style>body{font-family:sans-serif;}</style></head><body>");
        html.Append("<h1>Информация о клиенте</h1>");
        html.Append($"<p>Host: {context.Request.Host}</p>");
        html.Append($"<p>Path: {context.Request.Path}</p>");
        html.Append($"<p>Protocol: {context.Request.Protocol}</p>");
        html.Append($"<p>IP: {context.Connection.RemoteIpAddress}</p>");
        html.Append("<br/><a href='/'>Главная</a>");
        html.Append("</body></html>");
        await context.Response.WriteAsync(html.ToString());
    });
});

// Middleware для таблиц (Используем хитрый метод MapTable как в отчете)
MapTable<Guest>(app, "/guests", "Guests");
MapTable<Booking>(app, "/bookings", "Bookings");
MapTable<Room>(app, "/rooms", "Rooms");
MapTable<RoomType>(app, "/roomtypes", "RoomTypes");
MapTable<Employee>(app, "/employees", "Employees");
MapTable<AdditionalService>(app, "/services", "AdditionalServices");

// Middleware для /searchform1 (Cookies)
app.Map("/searchform1", appBuilder => {
    appBuilder.Run(async context => {
        // Читаем из куки
        var cookieVal = context.Request.Cookies["form1_cookie_data"];
        var formData = !string.IsNullOrEmpty(cookieVal)
            ? JsonSerializer.Deserialize<SearchModel>(cookieVal)
            : new SearchModel();

        // Если пришли новые данные (GET запрос с параметрами)
        if (context.Request.Query.ContainsKey("guestName"))
        {
            formData.GuestName = context.Request.Query["guestName"];
            formData.SearchType = context.Request.Query["searchType"];
            formData.FilterMode = context.Request.Query["filterMode"];

            // Пишем в куки
            var cookieOptions = new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(10) };
            context.Response.Cookies.Append("form1_cookie_data", JsonSerializer.Serialize(formData), cookieOptions);
        }

        context.Response.ContentType = "text/html;charset=utf-8";
        await context.Response.WriteAsync(BuildSearchFormHtml("/searchform1", formData, "Cookies"));
    });
});

// Middleware для /searchform2 (Session)
app.Map("/searchform2", appBuilder => {
    appBuilder.Run(async context => {
        // Читаем из сессии (через наш метод расширения)
        var formData = context.Session.Get<SearchModel>("form1_session_data") ?? new SearchModel();

        if (context.Request.Query.ContainsKey("guestName"))
        {
            formData.GuestName = context.Request.Query["guestName"];
            formData.SearchType = context.Request.Query["searchType"];
            formData.FilterMode = context.Request.Query["filterMode"];

            // Пишем в сессию
            context.Session.Set("form1_session_data", formData);
        }

        context.Response.ContentType = "text/html;charset=utf-8";
        await context.Response.WriteAsync(BuildSearchFormHtml("/searchform2", formData, "Session"));
    });
});

// Главная страница
app.Run(async (context) =>
{
    // --- ПРОГРЕВ КЭША (ЭТО ВАЖНО!) ---
    // Мы должны принудительно загрузить данные из БД в Кэш для ВСЕХ таблиц.
    var services = context.RequestServices;

    // 1. Гости
    services.GetService<ICachedService<Guest>>()?.AddEntities("Guests20");
    // 2. Бронирования
    services.GetService<ICachedService<Booking>>()?.AddEntities("Bookings20");

    // --- ДОБАВЬ ВОТ ЭТИ СТРОКИ, КОТОРЫХ НЕ ХВАТАЛО: ---

    // 3. Номера
    services.GetService<ICachedService<Room>>()?.AddEntities("Rooms20");

    // 4. Типы номеров
    services.GetService<ICachedService<RoomType>>()?.AddEntities("RoomTypes20");

    // 5. Сотрудники
    services.GetService<ICachedService<Employee>>()?.AddEntities("Employees20");

    // 6. Услуги (Обрати внимание, имя ключа должно совпадать с тем, что в MapTable - AdditionalServices)
    services.GetService<ICachedService<AdditionalService>>()?.AddEntities("AdditionalServices20");

    // --------------------------------------------------

    context.Response.ContentType = "text/html;charset=utf-8";
    var html = new StringBuilder();
    html.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><style>a{display:block;margin:5px;} body{font-family:sans-serif;padding:20px;}</style></head><body>");
    html.Append("<h1>Отель (Вариант 30)</h1>");
    html.Append("<h2>Данные в кэше (20 записей):</h2>");
    html.Append("<a href='/guests'>Гости</a>");
    html.Append("<a href='/bookings'>Бронирования</a>");
    html.Append("<a href='/rooms'>Номера</a>");
    html.Append("<a href='/roomtypes'>Типы номеров</a>");
    html.Append("<a href='/employees'>Сотрудники</a>");
    html.Append("<a href='/services'>Услуги</a>");
    html.Append("<h2>Поиск:</h2>");
    html.Append("<a href='/info'>Инфо</a>");
    html.Append("<a href='/searchform1'>Форма поиска (Cookies)</a>");
    html.Append("<a href='/searchform2'>Форма поиска (Session)</a>");
    html.Append("</body></html>");

    await context.Response.WriteAsync(html.ToString());
});

app.Run();

// --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (Внизу Program.cs) ---

// Магия из отчета: авто-создание страницы с таблицей
static void MapTable<T>(WebApplication app, string path, string title) where T : class
{
    app.Map(path, appBuilder => {
        appBuilder.Run(async context => {
            context.Response.ContentType = "text/html;charset=utf-8";
            // Получаем Generic сервис
            var cachedService = context.RequestServices.GetService<ICachedService<T>>();
            // Берем данные (ключ кэша = ИмяТаблицы + "20")
            var entities = cachedService?.GetEntities($"{title}20");

            var html = BuildTableHtml(entities, title);
            await context.Response.WriteAsync(html);
        });
    });
}

static string BuildTableHtml<T>(IEnumerable<T>? entities, string tableName)
{
    var html = new StringBuilder();
    html.Append($"<!DOCTYPE html><html><head><meta charset='utf-8' /><style>table{{border-collapse:collapse;width:100%;}} th,td{{border:1px solid #ddd;padding:8px;}} th{{background-color:#f2f2f2;}}</style></head><body>");
    html.Append($"<h1>{tableName}</h1>");

    if (entities == null || !entities.Any())
    {
        html.Append("<p>Нет данных в кэше. Проверьте БД.</p>");
    }
    else
    {
        html.Append("<table><thead><tr>");
        // Получаем имена свойств через рефлексию (автоматически создает заголовки)
        var properties = typeof(T).GetProperties().Where(p => !p.PropertyType.IsGenericType && !p.PropertyType.Name.StartsWith("ICollection")).ToArray();

        foreach (var prop in properties) html.Append($"<th>{prop.Name}</th>");
        html.Append("</tr></thead><tbody>");

        foreach (var entity in entities)
        {
            html.Append("<tr>");
            foreach (var prop in properties)
            {
                var val = prop.GetValue(entity)?.ToString() ?? "";
                html.Append($"<td>{val}</td>");
            }
            html.Append("</tr>");
        }
        html.Append("</tbody></table>");
    }
    html.Append("<br/><a href='/'>На главную</a></body></html>");
    return html.ToString();
}

static string BuildSearchFormHtml(string action, SearchModel formData, string type)
{
    return $@"<!DOCTYPE html><html><head><meta charset='utf-8'/></head><body>
    <h1>Поиск ({type})</h1>
    <form action='{action}' method='GET'>
        <label>Имя гостя:</label><br/>
        <input type='text' name='guestName' value='{formData.GuestName}'/><br/><br/>

        <label>Тип поиска:</label><br/>
        <select name='searchType'>
            <option value='Room' {(formData.SearchType == "Room" ? "selected" : "")}>По комнате</option>
            <option value='Phone' {(formData.SearchType == "Phone" ? "selected" : "")}>По телефону</option>
        </select><br/><br/>

        <label>Режим:</label><br/>
        <input type='radio' name='filterMode' value='Active' {(formData.FilterMode == "Active" ? "checked" : "")}> Активные
        <input type='radio' name='filterMode' value='Archive' {(formData.FilterMode == "Archive" ? "checked" : "")}> Архив<br/><br/>

        <input type='submit' value='Сохранить'/>
    </form>
    <br/><a href='/'>На главную</a></body></html>";
}