using Moq;
using SharedLibrary.Extensions;
using SharedLibrary.Localization;
using SharedLibrary.Utility;

namespace SharedLibraryTests.Utility;

public class GlobalizationExtensionsUtilityTests
{
    private readonly Mock<IGlobalizationService> _mockGlobalizationService;
    private readonly decimal _testAmount = 1234.56m;
    private readonly DateTime _testDate = new(2024, 3, 15, 14, 30, 0);
    private readonly string _testCulture = "fr-FR";

    public GlobalizationExtensionsUtilityTests()
    {
        _mockGlobalizationService = new Mock<IGlobalizationService>();
        GlobalizationDefaults.DefaultService = _mockGlobalizationService.Object;
    }

    [Fact]
    public void ToCurrency_WithExplicitService_CallsFormatCurrency()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatCurrency(_testAmount, null))
            .Returns("$1,234.56");

        // Act
        var result = _testAmount.ToCurrency(_mockGlobalizationService.Object);

        // Assert
        Assert.Equal("$1,234.56", result);
        _mockGlobalizationService.Verify(x => x.FormatCurrency(_testAmount, null), Times.Once);
    }

    [Fact]
    public void ToCurrency_WithDefaultService_CallsFormatCurrency()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatCurrency(_testAmount, null))
            .Returns("$1,234.56");

        // Act
        var result = _testAmount.ToCurrency();

        // Assert
        Assert.Equal("$1,234.56", result);
        _mockGlobalizationService.Verify(x => x.FormatCurrency(_testAmount, null), Times.Once);
    }

    [Fact]
    public void ToCurrency_WithCulture_CallsFormatCurrencyWithCulture()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatCurrency(_testAmount, _testCulture))
            .Returns("1.234,56 €");

        // Act
        var result = _testAmount.ToCurrency(_testCulture);

        // Assert
        Assert.Equal("1.234,56 €", result);
        _mockGlobalizationService.Verify(x => x.FormatCurrency(_testAmount, _testCulture), Times.Once);
    }

    [Fact]
    public void ToNumber_WithExplicitService_CallsFormatNumber()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatNumber(_testAmount, null))
            .Returns("1,234.56");

        // Act
        var result = _testAmount.ToNumber(_mockGlobalizationService.Object);

        // Assert
        Assert.Equal("1,234.56", result);
        _mockGlobalizationService.Verify(x => x.FormatNumber(_testAmount, null), Times.Once);
    }

    [Fact]
    public void ToFormattedDate_WithExplicitService_CallsFormatDate()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatDate(_testDate, null))
            .Returns("03/15/2024");

        // Act
        var result = _testDate.ToFormattedDate(_mockGlobalizationService.Object);

        // Assert
        Assert.Equal("03/15/2024", result);
        _mockGlobalizationService.Verify(x => x.FormatDate(_testDate, null), Times.Once);
    }

    [Fact]
    public void ToFormattedTime_WithExplicitService_CallsFormatTime()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatTime(_testDate, null))
            .Returns("14:30");

        // Act
        var result = _testDate.ToFormattedTime(_mockGlobalizationService.Object);

        // Assert
        Assert.Equal("14:30", result);
        _mockGlobalizationService.Verify(x => x.FormatTime(_testDate, null), Times.Once);
    }

    [Fact]
    public void ToFormattedDateTime_WithExplicitService_CallsFormatDateTime()
    {
        // Arrange
        _mockGlobalizationService.Setup(x => x.FormatDateTime(_testDate, null))
            .Returns("03/15/2024 14:30");

        // Act
        var result = _testDate.ToFormattedDateTime(_mockGlobalizationService.Object);

        // Assert
        Assert.Equal("03/15/2024 14:30", result);
        _mockGlobalizationService.Verify(x => x.FormatDateTime(_testDate, null), Times.Once);
    }

    [Fact]
    public void GlobalizationDefaults_WhenDefaultServiceNotSet_ThrowsInvalidOperationException()
    {
        // Arrange
        GlobalizationDefaults.DefaultService = null;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _testAmount.ToCurrency());
        Assert.Equal("Default globalization service not set", exception.Message);
    }
}