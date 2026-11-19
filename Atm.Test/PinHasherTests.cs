using ATM;
using FluentAssertions;

namespace Atm.Test;

public class PinHasherTests
{
    [Fact]
    public void VerifyPin_WhenPinMatches_ReturnsTrue()
    {
        //Arrange
        byte[] hash = PinHasher.HashPin("1245");

        //Act
        bool result = PinHasher.VerifyPin("1245", hash);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPin_WhenPinDoesNotMatch_ReturnFalse()
    {
        //Arrange
        byte[] hash = PinHasher.HashPin("2233");

        //Act
        bool result = PinHasher.VerifyPin("9999", hash);

        //Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("adbs")]
    [InlineData("")]
    public void HashPin_WhenPinIsInvalid_ThrowsArgumentException(string pin)
    {
        //Act
        Action act = () => PinHasher.HashPin(pin);

        //Assert
        act.Should().Throw<ArgumentException>();
    }
}
