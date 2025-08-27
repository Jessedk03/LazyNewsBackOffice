namespace NewsBackOffice.Models;


public class Article
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
    public string Image { get; set; }
    public Source Source { get; set; }
}