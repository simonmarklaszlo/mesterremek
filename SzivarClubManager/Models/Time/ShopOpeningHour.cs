using System;

namespace SzivarClubManager.Models.Time;

public class ShopOpeningHour : IModel
{
    public int Id { get; }
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly OpeningHour { get; }
    public TimeOnly ClosingHour { get; }

    public string DayOfWeekString => field ??= DayOfWeek.ToDisplayString();
    public virtual string ScheduleString => field ??= $"{OpeningHour:HH:mm} - {ClosingHour:HH:mm}";

    public ShopOpeningHour(int id, DayOfWeek dayOfWeek, TimeOnly openingHour, TimeOnly closingHour)
    {
        Id = id;
        DayOfWeek = dayOfWeek;
        OpeningHour = openingHour;
        ClosingHour = closingHour;
    }

    public string ToCopiableString() => $"{Id} {DayOfWeek.ToDisplayString()} {OpeningHour} {ClosingHour}";
}