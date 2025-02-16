# Globalization Service Access Patterns

Documentation for different approaches to access IGlobalizationService in .NET applications.

## Table of Contents

1. [Static Service Locator Pattern](#1-static-service-locator-pattern)
2. [HttpContext.RequestServices Approach](#2-httpcontext-requestservices-approach)
3. [IServiceProvider Injection](#3-iserviceprovider-injection)
4. [Ambient Context Pattern](#4-ambient-context-pattern)
5. [Thread-Static Approach](#5-thread-static-approach)
6. [Middleware + HttpContext.Items Approach](#6-middleware--httpcontextitems-approach)
7. [Scoped Service Factory Pattern](#7-scoped-service-factory-pattern)
8. [Extension Method Overloading with Default Service](#8-extension-method-overloading-with-default-service)

## 1. Static Service Locator Pattern

### Description

A static class that holds a single instance of IGlobalizationService accessible throughout the application.

### Implementation

```csharp
public static class GlobalizationServiceLocator
{
    private static IGlobalizationService? _service;

    public static void Initialize(IGlobalizationService service) => _service = service;

    public static IGlobalizationService Current => _service ??
        throw new InvalidOperationException("Service not initialized");
}

// Extension methods
public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value)
        => GlobalizationServiceLocator.Current.FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs or Startup.cs
public void Configure(IApplicationBuilder app)
{
    var service = app.ApplicationServices.GetRequiredService<IGlobalizationService>();
    GlobalizationServiceLocator.Initialize(service);
}
```

### Usage

```csharp
public class PaymentService
{
    public string FormatAmount(decimal amount)
    {
        return amount.ToCurrency();
    }
}
```

### Pros

- ✅ Simple implementation
- ✅ Easy to use throughout the application
- ✅ No need to pass service references
- ✅ Works in any context (web, console, etc.)
- ✅ Minimal code required

### Cons

- ❌ Violates dependency injection principles
- ❌ Hard to test (static state)
- ❌ Hidden dependencies
- ❌ Thread-safety concerns
- ❌ Cannot have multiple configurations

### Best Used When

- Building simple applications
- Prototyping
- Single configuration needed
- Testing is not a primary concern

## 2. HttpContext.RequestServices Approach

### Description

Utilizes ASP.NET Core's built-in dependency injection through HttpContext.

### Implementation

```csharp
public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value, HttpContext httpContext)
        => httpContext.RequestServices
            .GetRequiredService<IGlobalizationService>()
            .FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
builder.Services.AddScoped<IGlobalizationService, GlobalizationService>();
```

### Usage

```csharp
public class PaymentController : ControllerBase
{
    public IActionResult ProcessPayment(decimal amount)
    {
        string formatted = amount.ToCurrency(HttpContext);
        return Ok(formatted);
    }
}
```

### Pros

- ✅ Follows dependency injection principles
- ✅ Scoped to request lifetime
- ✅ No static state
- ✅ Easy to test controllers
- ✅ Access to request-specific configuration

### Cons

- ❌ Only works in web context
- ❌ Requires HttpContext access
- ❌ Not suitable for non-web applications
- ❌ Requires passing HttpContext around

### Best Used When

- Building web-only applications
- Need request-scoped services
- Working within controllers or middleware
- Need access to request-specific configuration

## 3. IServiceProvider Injection

### Description

Injects IServiceProvider to resolve the globalization service when needed.

### Implementation

```csharp
public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value, IServiceProvider services)
        => services.GetRequiredService<IGlobalizationService>()
            .FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
builder.Services.AddScoped<IGlobalizationService, GlobalizationService>();
```

### Usage

```csharp
public class PaymentService
{
    private readonly IServiceProvider _services;

    public PaymentService(IServiceProvider services)
    {
        _services = services;
    }

    public string FormatAmount(decimal amount)
    {
        return amount.ToCurrency(_services);
    }
}
```

### Pros

- ✅ Follows dependency injection principles
- ✅ Works in any context
- ✅ Flexible service resolution
- ✅ Can resolve different service lifetimes
- ✅ Access to all registered services

### Cons

- ❌ Requires passing IServiceProvider
- ❌ Can lead to service locator anti-pattern
- ❌ More verbose than direct injection
- ❌ May hide service dependencies

### Best Used When

- Need flexible service resolution
- Working with multiple service lifetimes
- Building pluggable components
- Need access to different services dynamically

## 4. Ambient Context Pattern

### Description

Creates a context that flows with async operations and holds the current service instance.

### Implementation

```csharp
public class GlobalizationContext : IDisposable
{
    private static AsyncLocal<IGlobalizationService?> _current = new();

    public static IGlobalizationService Current => _current.Value ??
        throw new InvalidOperationException("No globalization service in context");

    public GlobalizationContext(IGlobalizationService service)
    {
        _current.Value = service;
    }

    public void Dispose() => _current.Value = null;
}

public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value)
        => GlobalizationContext.Current.FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
builder.Services.AddScoped<IGlobalizationService, GlobalizationService>();
```

### Usage

```csharp
public class PaymentService
{
    private readonly IGlobalizationService _globalizationService;

    public PaymentService(IGlobalizationService globalizationService)
    {
        _globalizationService = globalizationService;
    }

    public void ProcessPayment(decimal amount)
    {
        using (new GlobalizationContext(_globalizationService))
        {
            string formatted = amount.ToCurrency();
            // Use formatted amount
        }
    }
}
```

### Pros

- ✅ Works well with async code
- ✅ Clean API at point of use
- ✅ Supports nested contexts
- ✅ Thread-safe
- ✅ Clear context boundaries

### Cons

- ❌ More complex implementation
- ❌ Requires careful scope management
- ❌ Must remember to dispose context
- ❌ Can be confusing for developers

### Best Used When

- Working with async code
- Need context flow across async boundaries
- Want clean API at point of use
- Need nested service contexts

## 5. Thread-Static Approach

### Description

Uses thread-static storage to maintain service instance per thread.

### Implementation

```csharp
public static class GlobalizationProvider
{
    [ThreadStatic]
    private static IGlobalizationService? _service;

    public static IGlobalizationService Current => _service ??
        throw new InvalidOperationException("Service not set");

    public static void SetService(IGlobalizationService service) => _service = service;
}

public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value)
        => GlobalizationProvider.Current.FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
builder.Services.AddScoped<IGlobalizationService, GlobalizationService>();
```

### Usage

```csharp
public class PaymentService
{
    public void ProcessPayment(IGlobalizationService service, decimal amount)
    {
        GlobalizationProvider.SetService(service);
        string formatted = amount.ToCurrency();
        // Use formatted amount
    }
}
```

### Pros

- ✅ Simple implementation
- ✅ Thread-safe for synchronous code
- ✅ Clean API at point of use
- ✅ No context objects to manage
- ✅ Works in any application type

### Cons

- ❌ Doesn't work well with async code
- ❌ State doesn't flow across thread boundaries
- ❌ Can lead to memory leaks
- ❌ Hard to test
- ❌ Hidden dependencies

### Best Used When

- Working with synchronous code only
- Simple threading scenarios
- No async operations
- Quick prototypes

## 6. Middleware + HttpContext.Items Approach

### Description

Stores the service instance in HttpContext.Items during request processing.

### Implementation

```csharp
public class GlobalizationMiddleware
{
    private const string ServiceKey = "GlobalizationService";
    private readonly RequestDelegate _next;

    public GlobalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IGlobalizationService service)
    {
        context.Items[ServiceKey] = service;
        await _next(context);
    }

    public static IGlobalizationService GetCurrent(HttpContext context)
        => (IGlobalizationService)context.Items[ServiceKey]!;
}

public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value, HttpContext context)
        => GlobalizationMiddleware.GetCurrent(context).FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
app.UseMiddleware<GlobalizationMiddleware>();
```

### Usage

```csharp
public class PaymentController : ControllerBase
{
    public IActionResult ProcessPayment(decimal amount)
    {
        string formatted = amount.ToCurrency(HttpContext);
        return Ok(formatted);
    }
}
```

### Pros

- ✅ Request-scoped service access
- ✅ No static state
- ✅ Works well with dependency injection
- ✅ Clear request boundaries
- ✅ Easy to test

### Cons

- ❌ Web-only solution
- ❌ Requires HttpContext
- ❌ Middleware setup required
- ❌ Not suitable for non-web applications

### Best Used When

- Building web applications
- Need request-scoped services
- Working with ASP.NET Core middleware
- Need clean request boundaries

## 7. Scoped Service Factory Pattern

### Description

Creates a factory that manages service instances and their lifetimes.

### Implementation

```csharp
public interface IGlobalizationServiceFactory
{
    IGlobalizationService GetService();
}

public class GlobalizationServiceFactory : IGlobalizationServiceFactory
{
    private readonly IServiceProvider _services;

    public GlobalizationServiceFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IGlobalizationService GetService()
        => _services.GetRequiredService<IGlobalizationService>();
}

public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value, IGlobalizationServiceFactory factory)
        => factory.GetService().FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
builder.Services.AddSingleton<IGlobalizationServiceFactory, GlobalizationServiceFactory>();
```

### Usage

```csharp
public class PaymentService
{
    private readonly IGlobalizationServiceFactory _factory;

    public PaymentService(IGlobalizationServiceFactory factory)
    {
        _factory = factory;
    }

    public string FormatAmount(decimal amount)
    {
        return amount.ToCurrency(_factory);
    }
}
```

### Pros

- ✅ Full control over service lifetime
- ✅ Clean dependency injection
- ✅ Easy to test
- ✅ Flexible configuration
- ✅ Clear dependencies

### Cons

- ❌ More complex implementation
- ❌ Additional abstraction layer
- ❌ More code to maintain
- ❌ Requires factory injection

### Best Used When

- Need control over service lifetime
- Complex service creation logic
- Multiple service configurations
- Clean architecture requirements

## 8. Extension Method Overloading with Default Service

### Description

Provides both default and explicit service options through method overloading.

### Implementation

```csharp
public static class GlobalizationDefaults
{
    public static IGlobalizationService? DefaultService { get; set; }

    public static IGlobalizationService Current => DefaultService ??
        throw new InvalidOperationException("Default service not set");
}

public static class GlobalizationExtensions
{
    public static string ToCurrency(this decimal value, IGlobalizationService service)
        => service.FormatCurrency(value);

    public static string ToCurrency(this decimal value)
        => GlobalizationDefaults.Current.FormatCurrency(value);
}
```

### Setup

```csharp
// In Program.cs
public void Configure(IApplicationBuilder app)
{
    GlobalizationDefaults.DefaultService = app.ApplicationServices
        .GetRequiredService<IGlobalizationService>();
}
```

### Usage

```csharp
public class PaymentService
{
    private readonly IGlobalizationService _customService;

    public PaymentService(IGlobalizationService customService = null)
    {
        _customService = customService;
    }

    public string FormatAmount(decimal amount)
    {
        // Using default service
        return amount.ToCurrency();
    }

    public string FormatAmountCustom(decimal amount)
    {
        // Using custom service
        return amount.ToCurrency(_customService);
    }
}
```

### Pros

- ✅ Flexible usage patterns
- ✅ Supports both default and custom services
- ✅ Easy to test
- ✅ Clear method signatures
- ✅ Good developer experience

### Cons

- ❌ Multiple methods to maintain
- ❌ Potential confusion about which overload to use
- ❌ Need to manage default service
- ❌ Slightly more complex API

### Best Used When

- Need both default and custom configurations
- Building reusable libraries
- Want flexibility in service usage
- Testing is important

## Conclusion

Each approach has its own use cases and trade-offs. Choose based on your specific needs:

- For web applications: Consider approaches 2 or 6
- For general-purpose libraries: Consider approaches 4 or 7
- For simple applications: Consider approaches 1 or 8
- For maximum testability: Consider approaches 4 or 7
- For async operations: Consider approaches 4 or 8
- For clean architecture: Consider approaches 7 or 8

Remember to consider:

- Application type (web, console, library)
- Testing requirements
- Async requirements
- Team experience
- Maintenance needs
