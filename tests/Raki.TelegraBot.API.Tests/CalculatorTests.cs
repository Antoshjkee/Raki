namespace Raki.TelegraBot.API.Tests;

using Raki.TegramBot.Core;

public class CalculatorTests
{
    [Fact]
    public void AddShouldReturnSum()
    {
        // Arrange
        var a = 3;
        var b = 5;

        // Act
        var result = Calculator.Add(a, b);

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void SubtractShouldReturnDifference()
    {
        // Arrange
        var a = 10;
        var b = 7;

        // Act
        var result = Calculator.Subtract(a, b);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void MultiplyShouldReturnProduct()
    {
        // Arrange
        var a = 4;
        var b = 7;

        // Act
        var result = Calculator.Multiply(a, b);

        // Assert
        Assert.Equal(28, result);
    }

    [Theory]
    [InlineData(6, 2, 3)]
    [InlineData(-6, 2, -3)]
    [InlineData(-6, -2, 3)]
    public void DivideShouldReturnQuotient(int a, int b, int expectedResult)
    {
        // Act
        var result = Calculator.Divide(a, b);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void DivideShouldThrowDivideByZeroException()
    {
        // Arrange
        var a = 10;
        var b = 0;

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => Calculator.Divide(a, b));
    }
}
