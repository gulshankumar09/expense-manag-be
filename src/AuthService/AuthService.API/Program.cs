using AuthService.API.Extensions;
using System.Threading.RateLimiting;
using SharedLibrary.Middleware;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

#if !DOCKER
// Only add service defaults when not running in a container (i.e., when running with Aspire)
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    builder.AddServiceDefaults();
}
#endif

// Add CORS
builder.Services.AddCustomCors(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Core Services in correct order
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEmailService(builder.Configuration);

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
       RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
         factory: partition => new FixedWindowRateLimiterOptions
         {
             AutoReplenishment = true,
             PermitLimit = 10,
             Window = TimeSpan.FromMinutes(1)
         }));
});

var app = builder.Build();

#if !DOCKER
// Only map default endpoints when not running in a container
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    app.MapDefaultEndpoints();
}
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseGlobalExceptionHandler();

// Use CORS before other middleware
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Add rate limiting
app.UseRateLimiter();

app.MapControllers();

// Initialize the database and create default roles and users
await app.InitializeDatabaseAsync();

app.Run();
