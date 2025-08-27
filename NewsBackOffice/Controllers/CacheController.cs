using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NewsBackOffice.Models;

namespace NewsBackOffice.Controllers;

public class CacheController : Controller
{
    private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

    public static void StoreInCache(string key, List<Article> articles)
    {
        Cache.Set(key, articles, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1)));
        Console.WriteLine("Articles saved to Cache");
    }

    public static async Task<List<Article>> GetArticlesFromCache(string key)
    {
        var articles = new List<Article>();

        if (Cache.TryGetValue(key, out List<Article>? result))
        {
            Console.WriteLine("Articles retrieved from Cache");
            return result;
        }
        
        return articles;
    }
}