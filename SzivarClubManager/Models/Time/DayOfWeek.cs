using System;

namespace SzivarClubManager.Models.Time;

public enum DayOfWeek
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}

public static class DayOfWeekExtensions
{
    extension(DayOfWeek d)
    {
        public string ToDisplayString() => d switch
        {
            DayOfWeek.Monday => "Hétfő",
            DayOfWeek.Tuesday => "Kedd",
            DayOfWeek.Wednesday => "Szerda",
            DayOfWeek.Thursday => "Csütörtök",
            DayOfWeek.Friday => "Péntek",
            DayOfWeek.Saturday => "Szombat",
            DayOfWeek.Sunday => "Vasárnap",
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };

        public static DayOfWeek? TryMatch(string day) => day switch
        {
            "Hétfő" => DayOfWeek.Monday,
            "Kedd" => DayOfWeek.Tuesday,
            "Szerda" => DayOfWeek.Wednesday,
            "Csütörtök" => DayOfWeek.Thursday,
            "Péntek" => DayOfWeek.Friday,
            "Szombat" => DayOfWeek.Saturday,
            "Vasárnap" => DayOfWeek.Sunday,
            _ => null
        };

        public static DayOfWeek FromId(int id) => (DayOfWeek)id;
    }
}