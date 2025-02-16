# Localization and Globalization Guide

This guide explains how to implement and use localization and globalization features in the application.

## Table of Contents

- [Overview](#overview)
- [Configuration](#configuration)
- [Implementation](#implementation)
- [Usage Examples](#usage-examples)
- [Supported Cultures](#supported-cultures)
- [Best Practices](#best-practices)

## Overview

The application supports both localization (translation of text) and globalization (formatting of dates, numbers, and currencies) through a comprehensive implementation using .NET's built-in features and custom services.

### Features

- Multi-language support
- Culture-specific formatting for:
  - Dates and times
  - Numbers
  - Currencies
- Automatic culture detection
- Fallback mechanisms
- Resource-based translations

## Configuration

### 1. NuGet Packages

Add the following packages to your project:

```xml
<PackageReference Include="Microsoft.Extensions.Localization" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Localization" Version="2.2.0" />
```

### 2. appsettings.json Configuration

Add the following section to your `appsettings.json`:

```json
{
  "Localization": {
    "DefaultCulture": "en-US",
    "SupportedCultures": ["en-US", "es-ES", "fr-FR", "de-DE", "hi-IN"],
    "ResourcesPath": "Resources",
    "UseUserPreferredCulture": true,
    "FallBackToParentCulture": true,
    "ThrowOnMissingTranslation": false,
    "DefaultCurrencyFormat": "C2",
    "DefaultDateFormat": "MM/dd/yyyy",
    "DefaultTimeFormat": "HH:mm",
    "DefaultNumberFormat": "N2",
    "UseInvariantCulture": false
  }
}
```

### 3. Project Structure

Create the following folders in your project:

```
/Resources/
  - SharedResource.en-US.resx
  - SharedResource.es-ES.resx
  - etc...
/Localization/
  - LocalizationService.cs
  - GlobalizationService.cs
  - ILocalizationService.cs
  - IGlobalizationService.cs
```

## Implementation

### 1. Program.cs Setup

Add the following to your `Program.cs`:

```csharp
// Add localization services
builder.Services.AddAppLocalization(builder.Configuration);

// Add localization to controllers
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .AddViewLocalization();

// Use localization middleware (before other middleware)
app.UseAppLocalization();
```

### 2. Resource Files

Create resource files for each supported culture. Example structure for `SharedResource.en-US.resx`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Welcome" xml:space="preserve">
    <value>Welcome</value>
  </data>
  <data name="Error_Required" xml:space="preserve">
    <value>This field is required</value>
  </data>
  <!-- Add more translations -->
</root>
```

## Usage Examples

### 1. In Controllers

```csharp
public class YourController : ControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IGlobalizationService _globalizationService;

    public YourController(
        ILocalizationService localizationService,
        IGlobalizationService globalizationService)
    {
        _localizationService = localizationService;
        _globalizationService = globalizationService;
    }

    [HttpGet]
    public IActionResult GetFormattedData()
    {
        var welcomeMessage = _localizationService.GetLocalizedString("Welcome");
        var amount = _globalizationService.FormatCurrency(100.50m);
        var date = _globalizationService.FormatDate(DateTime.Now);

        return Ok(new {
            message = welcomeMessage,
            amount = amount,
            date = date
        });
    }
}
```

### 2. In Models

```csharp
public class ExpenseModel
{
    [Required(ErrorMessage = "Error_Required")]
    public decimal Amount { get; set; }

    public string FormattedAmount =>
        _globalizationService.FormatCurrency(Amount);

    public DateTime Date { get; set; }

    public string FormattedDate =>
        _globalizationService.FormatDate(Date);
}
```

### 3. Changing Culture

```csharp
// Via query string
https://yourapp.com/api/data?culture=es-ES

// Via header
Accept-Language: es-ES

// Via cookie
.AspNetCore.Culture=c=es-ES|uic=es-ES
```

## Supported Cultures

The application currently supports the following cultures:

- English (en-US)
- Spanish (es-ES)
- French (fr-FR)
- German (de-DE)
- Hindi (hi-IN)

To add a new culture:

1. Add the culture code to `SupportedCultures` in `appsettings.json`
2. Create corresponding resource files
3. Add any culture-specific formatting requirements

## Best Practices

1. **Resource Keys**

   - Use consistent naming conventions
   - Group related keys (e.g., `Error_`, `Success_`, `Validation_`)
   - Keep keys descriptive and unique

2. **Formatting**

   - Use standard format strings when possible
   - Consider regional differences in date formats
   - Handle currency decimals appropriately

3. **Culture Handling**

   - Always provide fallback values
   - Test with different cultures
   - Consider RTL languages if supporting them

4. **Performance**

   - Cache frequently used translations
   - Use appropriate culture providers
   - Consider resource file size

5. **Maintenance**
   - Keep resource files organized
   - Document custom formats
   - Regular validation of translations

## Troubleshooting

Common issues and solutions:

1. **Missing Translations**

   - Check resource file names match culture codes
   - Verify resource file Build Action is set to "Embedded Resource"
   - Check `ResourcesPath` configuration

2. **Incorrect Formatting**

   - Verify culture code is supported
   - Check format string patterns
   - Ensure proper culture initialization

3. **Culture Not Changing**
   - Verify middleware order
   - Check culture provider configuration
   - Validate cookie/header values

## Testing

1. **Unit Tests**

```csharp
[Fact]
public void FormatCurrency_WithUSCulture_ReturnsCorrectFormat()
{
    var service = new GlobalizationService(options);
    var result = service.FormatCurrency(100.50m, "en-US");
    Assert.Equal("$100.50", result);
}
```

2. **Integration Tests**

```csharp
[Fact]
public async Task GetData_WithSpanishCulture_ReturnsLocalizedData()
{
    var response = await client.GetAsync("/api/data?culture=es-ES");
    var result = await response.Content.ReadAsStringAsync();
    Assert.Contains("€", result);
}
```
