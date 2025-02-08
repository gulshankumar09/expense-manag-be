using AuthService.API.Extensions;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Core Services in correct order
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
       RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
         factory: partition => new FixedWindowRateLimiterOptions
         {
             AutoReplenishment = true,
             PermitLimit = 10,
             Window = TimeSpan.FromMinutes(1)
         }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Add rate limiting
app.UseRateLimiter();

app.MapControllers();

// Initialize the database and create default roles and users
await app.InitializeDatabaseAsync();

app.Run();
