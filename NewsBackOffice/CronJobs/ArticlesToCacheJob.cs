using NewsBackOffice.Controllers;
using Quartz;

namespace NewsBackOffice.CronJobs;

public class ArticlesToCacheJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        MainController.ArticleToCache();
        return Task.CompletedTask;
    }
}