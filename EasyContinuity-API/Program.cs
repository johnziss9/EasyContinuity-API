using EasyContinuity_API.Data;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

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
