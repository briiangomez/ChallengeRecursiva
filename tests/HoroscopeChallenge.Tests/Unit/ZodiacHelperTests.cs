using FluentAssertions;
using HoroscopeChallenge.Api.Domain.Helpers;
using Xunit;

namespace HoroscopeChallenge.Tests.Unit;

public class ZodiacHelperTests
{

    [Theory]
    [InlineData(1,  1,  "capricornio")]  // 1 ene: Capricornio
    [InlineData(1,  19, "capricornio")]  // 19 ene: aún Capricornio
    [InlineData(1,  20, "acuario")]      // 20 ene: primer día de Acuario
    [InlineData(2,  18, "acuario")]      // 18 feb: aún Acuario
    [InlineData(2,  19, "piscis")]       // 19 feb: primer día de Piscis
    [InlineData(3,  20, "piscis")]       // 20 mar: aún Piscis
    [InlineData(3,  21, "aries")]        // 21 mar: primer día de Aries
    [InlineData(4,  19, "aries")]        // 19 abr: aún Aries
    [InlineData(4,  20, "tauro")]        // 20 abr: primer día de Tauro
    [InlineData(5,  20, "tauro")]        // 20 may: aún Tauro
    [InlineData(5,  21, "geminis")]      // 21 may: primer día de Géminis
    [InlineData(6,  20, "geminis")]      // 20 jun: aún Géminis
    [InlineData(6,  21, "cancer")]       // 21 jun: primer día de Cáncer
    [InlineData(7,  15, "cancer")]       // 15 jul: Cáncer (centro)
    [InlineData(7,  22, "cancer")]       // 22 jul: aún Cáncer
    [InlineData(7,  23, "leo")]          // 23 jul: primer día de Leo
    [InlineData(8,  1,  "leo")]          // 1 ago: Leo
    [InlineData(8,  22, "leo")]          // 22 ago: aún Leo
    [InlineData(8,  23, "virgo")]        // 23 ago: primer día de Virgo
    [InlineData(9,  22, "virgo")]        // 22 sep: aún Virgo
    [InlineData(9,  23, "libra")]        // 23 sep: primer día de Libra
    [InlineData(10, 22, "libra")]        // 22 oct: aún Libra
    [InlineData(10, 23, "escorpio")]     // 23 oct: primer día de Escorpio
    [InlineData(11, 21, "escorpio")]     // 21 nov: aún Escorpio
    [InlineData(11, 22, "sagitario")]    // 22 nov: primer día de Sagitario
    [InlineData(12, 21, "sagitario")]    // 21 dic: aún Sagitario
    [InlineData(12, 22, "capricornio")] // 22 dic: primer día de Capricornio (cruza año)
    [InlineData(12, 31, "capricornio")] // 31 dic: Capricornio
    public void GetSign_ShouldReturnCorrectSign(int month, int day, string expectedSign)
    {
        var birthDate = new DateOnly(2000, month, day);
        var sign = ZodiacHelper.GetSign(birthDate);
        sign.Should().Be(expectedSign);
    }


    [Fact]
    public void GetDaysToBirthday_WhenBirthdayIsToday_ShouldReturnZero()
    {
        var today     = new DateOnly(2025, 7, 15);
        var birthDate = new DateOnly(1990, 7, 15);

        ZodiacHelper.GetDaysToBirthday(birthDate, today).Should().Be(0);
    }

    [Fact]
    public void GetDaysToBirthday_WhenBirthdayIsTomorrow_ShouldReturnOne()
    {
        var today     = new DateOnly(2025, 7, 14);
        var birthDate = new DateOnly(1990, 7, 15);

        ZodiacHelper.GetDaysToBirthday(birthDate, today).Should().Be(1);
    }

    [Fact]
    public void GetDaysToBirthday_WhenBirthdayAlreadyPassedThisYear_ShouldReturnNextYear()
    {
        var today     = new DateOnly(2025, 8, 1);
        var birthDate = new DateOnly(1990, 7, 15); 

        var days = ZodiacHelper.GetDaysToBirthday(birthDate, today);

        var expected = new DateOnly(2026, 7, 15).DayNumber - today.DayNumber;
        days.Should().Be(expected);
    }

    [Fact]
    public void GetDaysToBirthday_LeapYearBirthday_ShouldHandleCorrectly()
    {
        var birthDate = new DateOnly(2000, 2, 29);
        var today     = new DateOnly(2025, 2, 28); 

        var act = () => ZodiacHelper.GetDaysToBirthday(birthDate, today);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetDaysToBirthday_CrossYearBoundary_ShouldCalculateCorrectly()
    {
        var today     = new DateOnly(2025, 12, 31);
        var birthDate = new DateOnly(1990,  1,  1); 

        var days = ZodiacHelper.GetDaysToBirthday(birthDate, today);
        days.Should().Be(1);
    }
}
