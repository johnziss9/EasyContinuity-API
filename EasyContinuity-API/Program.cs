using EasyContinuity_API.Data;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ECDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ECPostgresConnection")));

builder.Services.AddScoped<ISpaceService, SpaceService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<ISnapshotService, SnapshotService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<ICloudinaryStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IImageCompressionService, ImageCompressionService>();
builder.Services.AddScoped<AttachmentCleanupService>();

// Add Quartz packages
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("AttachmentCleanup");
    
    q.AddJob<AttachmentCleanupJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("AttachmentCleanup-Trigger")
        .WithCronSchedule("0 */2 * * * ?")
        // For production: change to run every 7 days at midnight:
        // .WithCronSchedule("0 0 0 */7 * ?")
        );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Urls.Add("http://0.0.0.0:80");

app.Run();
