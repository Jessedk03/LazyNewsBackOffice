using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsBackOffice.Models;
using NewsBackOffice.Properties;
using NewsBackOffice.Repositories;
using Newtonsoft.Json;

namespace NewsBackOffice.Controllers;

[Route("api/index")]
[ApiController]
public class MainController : Controller
{
    private const string Key = "articles";

    private readonly ArticlesRepository _articlesRepository =
        new ArticlesRepository(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));

    // Index
    public async Task<List<Article>> Index()
    {
        var articles = await CacheController.GetArticlesFromCache(Key);

        if (articles.IsNullOrEmpty())
        {
            // get articles from database.
            articles = await _articlesRepository.GetTenLatestArticles();
            // cache articles from database.
            CacheController.StoreInCache(Key, articles);
        }

        Console.WriteLine($"Articles count: {articles.Count}");
        return articles;
    }

    public static async Task ArticleToCache()
    {
        var articlesRepository =
            new ArticlesRepository(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));
        Console.WriteLine("\n[" + DateTime.Now.ToString("HH:mm:ss") + "] Getting the newest articles...");
        var articles = await GetArticles();

        // Caching articles
        CacheController.StoreInCache(Key, articles);

        // Getting existing articles
        var existingArticles = await articlesRepository.GetArticleId(articles);
        Console.WriteLine(existingArticles);

        var newArticles = articles.Where(article => !existingArticles.Contains(article.Id));

        foreach (var article in newArticles)
        {
            await articlesRepository.AddArticle(article);
        }
    }

    private static async Task<List<Article>> GetArticles()
    {
        // TODO: ENV variables
        string uri =
            "https://gnews.io/api/v4/top-headlines?lang=nl&category=world&apikey=" + Environment.GetEnvironmentVariable("GNEWS_API_KEY");
        var articles = new List<Article>();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "NewsBackOffice");

        var json = client.GetStringAsync(uri).Result;
        var dynamicObject = JsonConvert.DeserializeObject<dynamic>(json);

        if (dynamicObject == null)
        {
            return articles;
        }

        foreach (var articleRequest in dynamicObject.articles)
        {
            var article = new Article();
            var source = new Source();

            article.Id = articleRequest.id;
            article.Title = articleRequest.title;
            article.Description = articleRequest.description;
            article.Content = articleRequest.content;
            article.Url = articleRequest.url;
            article.Image = articleRequest.image;
            source.Id = articleRequest.source.id;
            source.Name = articleRequest.source.name;
            source.Url = articleRequest.source.url;
            article.Source = source;
            articles.Add(article);
        }

        return articles;
    }
}