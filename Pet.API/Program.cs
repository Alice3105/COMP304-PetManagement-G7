using Pet.API.Services;
using Pet.API.Services.Interfaces;
using Pet.API.Repositories;
using Pet.API.Repositories.Interfaces;
using Pet.API.Models.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DynamoDB Context
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// Register File Upload Service
builder.Services.AddScoped<IFileUploadService, S3FileUploadService>();

// Register Repositories
builder.Services.AddScoped<IPetRepository, DynamoDBPetRepository>();
builder.Services.AddScoped<IAdoptionRepository, DynamoDBAdoptionRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, DynamoDBMedicalRecordRepository>();

// Register Data Seeding Service
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();

builder.Services.AddControllers();

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(RoleConstants.Admin)));
    
    options.AddPolicy("StaffOrAdmin", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole(RoleConstants.Admin) || context.User.IsInRole(RoleConstants.Staff)));
    
    options.AddPolicy("Public", policy => 
        policy.RequireAssertion(context => true));
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Seed data on startup - this runs BEFORE app.Run() to ensure it completes
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== Starting data seeding on application startup ===");

using (var scope = app.Services.CreateScope())
{
    var seedingService = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();
    try
    {
        await seedingService.SeedDataAsync();
        logger.LogInformation("=== Data seeding completed successfully ===");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "=== An error occurred while seeding the database ===");
    }
}

logger.LogInformation("=== Application is ready to accept requests ===");
app.Run();
