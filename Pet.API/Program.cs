using Pet.API.Services;
using Pet.API.Services.Interfaces;
using Pet.API.Repositories;
using Pet.API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DynamoDB Context
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// Register File Upload Service
builder.Services.AddScoped<IFileUploadService, S3FileUploadService>();

// Register Pet Repository 
builder.Services.AddScoped<IPetRepository, DynamoDBPetRepository>();

builder.Services.AddControllers();

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

app.Run();
