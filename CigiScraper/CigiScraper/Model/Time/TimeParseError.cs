namespace CigiScraper.Model.Time;

[Flags]
public enum TimeParseError
{
    None = 0,
    OverflowHour = 1 << 0,
    OverflowMinute = 1 << 1,
    RecursionOverflow = 1 << 2,
    ClosingSoonerThanOpening = 1 << 3,
    InvalidHour = 1 << 4,
    InvalidMinute = 1 << 5,
    SingleTag = 1 << 6,
    MoreThan2Digits = 1 << 7,
    Fallback = 1 << 8,
    UnabledToParse = 1 << 9,
}
