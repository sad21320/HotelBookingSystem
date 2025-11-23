using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using HotelBookingSystem.Data;

namespace HotelBookingSystem.Services
{
    public class CachedService<T> : ICachedService<T> where T : class
    {
        private readonly HotelDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public CachedService(HotelDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public void AddEntities(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<T> entities))
            {
                // Берем данные строго из БД
                entities = _dbContext.Set<T>().Take(rowsNumber).ToList();

                _memoryCache.Set(cacheKey, entities, new MemoryCacheEntryOptions
                {
                    // Время кэширования для Варианта 30: 2 * 30 + 240 = 300 секунд
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300)
                });
            }
        }

        public IEnumerable<T> GetEntities(string cacheKey, int rowsNumber = 20)
        {
            _memoryCache.TryGetValue(cacheKey, out IEnumerable<T> entities);
            return entities;
        }
    }
}