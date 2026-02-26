namespace HoroscopeChallenge.Api.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    DateOnly BirthDate,
    string Password
);

public record LoginRequest(
    string Username,
    string Password
);

public record LoginResponse(
    string Token,
    string Username,
    string Email
);

public record UserProfileResponse(
    int Id,
    string Username,
    string Email,
    DateOnly BirthDate,
    string ZodiacSign,
    int DaysToBirthday,
    DateTime CreatedAt
);

public record UpdateProfileRequest(
    string Email,
    DateOnly BirthDate
);

public record HoroscopeTodayResponse(
    string Sign,
    string Horoscope,
    int DaysToBirthday,
    DateOnly Date
);

public record MostQueriedSignResponse(
    string Sign,
    int QueryCount
);
