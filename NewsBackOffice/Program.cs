using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NewsBackOffice.Models;
using NewsBackOffice.CronJobs;
using Quartz;
using NewsBackOffice.Properties;

const string lazyNewsFrontEndOrigins = "_lazyNewsFrontEndOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: lazyNewsFrontEndOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("PutArticlesInCache");
    q.AddJob<ArticlesToCacheJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever())
    );
});
builder.Services.AddQuartzHostedService();

builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<ArticleContext>(opt =>
    opt.UseInMemoryDatabase("Articles"));
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    DotEnv.Load("/Users/jessedekoe/Projects/LazyBackOfficeProdMock/.env");
}
else
{
    // production
    DotEnv.Load("/home/stoppenkast/docker-newsbackoffice/.env");
}
app.UseHttpsRedirection();

app.UseCors(lazyNewsFrontEndOrigins);

app.MapControllers();

app.Run();