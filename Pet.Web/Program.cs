using Pet.Web.Services;
using Pet.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add cookie authentication for role-based authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";               // Redirect here if not logged in
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if logged in but not allowed
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Add session support for authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor for session access in services
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for Pet.API localhost
// string apiBaseUrl = builder.Configuration["PetApiBaseUrl"] ?? "https://localhost:7148";
// builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
// {
//     client.BaseAddress = new Uri(apiBaseUrl);
// });

// builder.Services.AddHttpClient<IPetApiService, PetApiService>(client =>
// {
//     client.BaseAddress = new Uri(apiBaseUrl);
// });

// builder.Services.AddHttpClient<IAdoptionApiService, AdoptionApiService>(client =>
// {
//     client.BaseAddress = new Uri(apiBaseUrl);
// });

// builder.Services.AddHttpClient<IMedicalRecordApiService, MedicalRecordApiService>(client =>
// {
//     client.BaseAddress = new Uri(apiBaseUrl);
// });

// Configure HttpClient for Pet.API through Apigee
string apiBaseUrl = builder.Configuration["PetApiBaseUrl"] ?? "https://localhost:7148";
string apiKey = builder.Configuration["ApigeeApiKey"] ?? "";

// Log the configuration value being read
System.Console.WriteLine($"[DEBUG] PetApiBaseUrl from config: '{apiBaseUrl}'");

// Ensure the base URL ends with a trailing slash for proper URL combination
// HttpClient requires BaseAddress to end with '/' when combining with relative paths
if (!apiBaseUrl.EndsWith("/"))
{
    apiBaseUrl += "/";
    System.Console.WriteLine($"[DEBUG] PetApiBaseUrl after adding trailing slash: '{apiBaseUrl}'");
}

// Configure all API services with base URL and API key header
Action<HttpClient> configureClient = client =>
{
    var baseUri = new Uri(apiBaseUrl);
    client.BaseAddress = baseUri;
    System.Console.WriteLine($"[DEBUG] HttpClient BaseAddress set to: '{client.BaseAddress}'");
    System.Console.WriteLine($"[DEBUG] BaseAddress AbsoluteUri: '{client.BaseAddress.AbsoluteUri}'");
    
    // Add API key header for Apigee (if configured)
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }
};

builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(configureClient);
builder.Services.AddHttpClient<IPetApiService, PetApiService>(configureClient);
builder.Services.AddHttpClient<IAdoptionApiService, AdoptionApiService>(configureClient);
builder.Services.AddHttpClient<IMedicalRecordApiService, MedicalRecordApiService>(configureClient);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
