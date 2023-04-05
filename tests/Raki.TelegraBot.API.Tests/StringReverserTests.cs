namespace Raki.TelegraBot.API.Tests;

using Raki.TegramBot.Core;

public class StringReverserTests
{
    [Theory]
    [InlineData("hello", "olleh")]
    [InlineData("world", "dlrow")]
    [InlineData("abcde", "edcba")]
    public void ReverseString_InputReversed_ReturnsReversedString(string input, string expected)
    {
        // Act
        var actual = StringReverser.ReverseString(input);

        // Assert
        Assert.Equal(expected, actual);
    }
}
