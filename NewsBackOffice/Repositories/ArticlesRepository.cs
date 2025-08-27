using MySqlConnector;
using NewsBackOffice.Models;

namespace NewsBackOffice.Repositories;

public class ArticlesRepository
{
    private readonly string _connectionString;

    public ArticlesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Article>> GetTenLatestArticles()
    {
        await using var connection = new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = new MySqlCommand("SELECT * FROM articles;", connection);
        await using var reader = await command.ExecuteReaderAsync();

        var articles = new List<Article>();
        while (await reader.ReadAsync())
        {
            var article = new Article
            {
                Id = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                Content = reader.GetString(4),
                Url = reader.GetString(5),
                Image = reader.GetString(6),
                Source = new Source
                {
                    Id = reader.GetString(7),
                    Name = reader.GetString(8),
                    Url = reader.GetString(9)
                }
            };
            articles.Add(article);
        }

        return articles;
    }

    public async Task<List<string>> GetArticleId(List<Article> articleIds)
    {
        var articleIdPlaceHolder = string.Join(", ", Enumerable.Repeat("?", articleIds.Count));
        
        await using var connection = new MySqlConnection(_connectionString);
        
        await connection.OpenAsync();

        await using var command = new MySqlCommand($"SELECT article_id FROM articles WHERE article_id IN ({articleIdPlaceHolder});", connection);
        
        foreach (var articleId in articleIds)
        {
            command.Parameters.AddWithValue("@article_id", articleId.Id);
        }
        
        await using var reader = await command.ExecuteReaderAsync();
        
        var existingArticles = new List<string>();
        while (await reader.ReadAsync())
        {
            var existingArticleId = reader.GetString(0);
            existingArticles.Add(existingArticleId);
        }

        return existingArticles;
    }

    public async Task AddArticle(Article article)
    {
        Console.WriteLine($"Adding article: {article.Id} - {article.Title}");
        
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        
        cmd.CommandText =
            @"INSERT INTO articles (article_id, article_title, article_description, article_content, article_url, article_image_url, article_source_id, article_source_name, article_source_url) VALUES (@article_id, @article_title, @article_description, @article_content, @article_url, @article_image_url, @article_source_id, @article_source_name, @article_source_url)";
        
        cmd.Parameters.AddWithValue("@article_id", article.Id);
        cmd.Parameters.AddWithValue("@article_title", article.Title);
        cmd.Parameters.AddWithValue("@article_description", article.Description);
        cmd.Parameters.AddWithValue("@article_content", article.Content);
        cmd.Parameters.AddWithValue("@article_url", article.Url);
        cmd.Parameters.AddWithValue("@article_image_url", article.Image);
        cmd.Parameters.AddWithValue("@article_source_id", article.Source.Id);
        cmd.Parameters.AddWithValue("@article_source_name", article.Source.Name);
        cmd.Parameters.AddWithValue("@article_source_url", article.Source.Url);
        await cmd.ExecuteNonQueryAsync();
    }
}