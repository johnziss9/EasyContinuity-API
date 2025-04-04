using System.Text;
using EasyContinuity_API.Data;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var jwtSettings = new JwtSettings
{
    Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY environment variable is not set."),
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "No JWT Issuer",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "No JWT Audience",
    ExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"), out int minutes) ? minutes : 60
};

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});


// Add Quartz packages
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("AttachmentCleanup");

    q.AddJob<AttachmentCleanupJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("AttachmentCleanup-Trigger")
        .WithCronSchedule("0 0 0 */2 * ?") // This is every 2 days at midnight
        // .WithCronSchedule("0 */2 * * * ?") // This is every 2 minutes used for testing
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

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.MapControllers();

app.Urls.Add("http://0.0.0.0:80");

app.Run();
