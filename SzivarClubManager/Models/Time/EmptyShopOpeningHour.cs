using System;

namespace SzivarClubManager.Models.Time;

public class EmptyShopOpeningHour : ShopOpeningHour
{
    public override string ScheduleString => field ??= "-";
    public EmptyShopOpeningHour(int id, DayOfWeek dayOfWeek) : base(id, dayOfWeek, TimeOnly.MaxValue, TimeOnly.MinValue) { }
}