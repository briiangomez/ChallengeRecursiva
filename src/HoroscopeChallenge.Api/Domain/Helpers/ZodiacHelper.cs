namespace HoroscopeChallenge.Api.Domain.Helpers;
public static class ZodiacHelper
{
    private static readonly (int Month, int Day, string Sign)[] _ranges =
    [
        (1,  20, "acuario"),
        (2,  19, "piscis"),
        (3,  21, "aries"),
        (4,  20, "tauro"),
        (5,  21, "geminis"),
        (6,  21, "cancer"),
        (7,  23, "leo"),
        (8,  23, "virgo"),
        (9,  23, "libra"),
        (10, 23, "escorpio"),
        (11, 22, "sagitario"),
        (12, 22, "capricornio"),
    ];

    public static string GetSign(DateOnly birthDate)
    {
        int month = birthDate.Month;
        int day   = birthDate.Day;

        foreach (var (m, d, sign) in _ranges)
        {
            if (month == m && day >= d) return sign.ToUpper();
            if (month == m && day <  d) break;   
        }

        for (int i = _ranges.Length - 1; i >= 0; i--)
        {
            if (_ranges[i].Month < month || (_ranges[i].Month == month && _ranges[i].Day <= day))
                return _ranges[i].Sign.ToUpper();
        }

        return "CAPRICORNIO";
    }

    /// <summary>
    /// Calcula los días que faltan para el próximo cumpleaños.
    /// Devuelve 0 si hoy es el cumpleaños.
    /// </summary>
    public static int GetDaysToBirthday(DateOnly birthDate, DateOnly today)
    {
        DateOnly nextBirthday;
        try
        {
            nextBirthday = new DateOnly(today.Year, birthDate.Month, birthDate.Day);
        }
        catch (ArgumentOutOfRangeException)
        {
            nextBirthday = new DateOnly(today.Year, 2, 28);
        }

        if (nextBirthday < today)
        {
            try
            {
                nextBirthday = new DateOnly(today.Year + 1, birthDate.Month, birthDate.Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                nextBirthday = new DateOnly(today.Year + 1, 2, 28);
            }
        }

        return nextBirthday.DayNumber - today.DayNumber;
    }
}
