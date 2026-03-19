using System;
using System.Collections.Generic;
using System.Linq;

namespace SzivarClubManager.Models.Time;

public class ShopOpeningSchedule
{
    public IReadOnlyList<ShopOpeningHour> OpeningHours => _openingHours;
    private readonly List<ShopOpeningHour> _openingHours;

    public ShopOpeningSchedule(int shopId, List<ShopOpeningHour> openingHours)
    {
        _openingHours = openingHours;
        if (_openingHours.Count != 7)
        {
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                if (_openingHours.All(x => x.DayOfWeek != day)) _openingHours.Add(new EmptyShopOpeningHour(shopId, day));
            }
        }

        _openingHours.Sort(CompareByDayOfWeek);
    }

    private static readonly Comparison<ShopOpeningHour> CompareByDayOfWeek = (a, b) => a.DayOfWeek.CompareTo(b.DayOfWeek);
}