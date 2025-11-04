using System.Diagnostics;
using System.Text.RegularExpressions;
using CigiScraper.LocalData;

namespace CigiScraper.Model.Time;

public partial class OpeningHours
{
    public TimeOnly Opening { get; }
    public TimeOnly Closing { get; }
    private static readonly char[] Separator = [':', '.', ' '];
    private const int SusgeMinute = 39;
    private const int SusgeHour = 1;

    private static TimeOnly MostCommonOpening => new TimeOnly(6, 0);
    private static TimeOnly MostCommonClosing => new TimeOnly(20, 0);

    public OpeningHours(TimeOnly opening, TimeOnly closing)
    {
        Opening = opening;
        Closing = closing;
    }


    /// <returns>null ha zárva, különben valid</returns>
    public static OpeningHours? Parse(string fullTime, string url, List<OpeningSchedule> context)
    {
        if (IsClosedRegex().IsMatch(fullTime))
        {
            return null;
        }

        fullTime = fullTime.Trim();
        var split = fullTime.Split('-');

        if (split.Length == 1)
        {
            if (!ContainsNumber().IsMatch(split[0]))
            {
                return null;
            }

            split = fullTime.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        if (split.Length > 2)
        {
            split = [split[0], split[^1]];
        }

        if (split.Length != 2)
        {
            Logger2.LogErrorTime(fullTime, TimeParseError.UnabledToParse, url);
            return new OpeningHours(MostCommonOpening, MostCommonClosing);
        }

        var (opening, openingErr) = ParseOneSide(split[0], context);
        var (closing, closingErr) = ParseOneSide(split[1], context, opening);

        const TimeParseError ignoreMask = TimeParseError.None | TimeParseError.Fallback | TimeParseError.UnabledToParse;

        if (openingErr.HasFlag(TimeParseError.UnabledToParse)) Logger2.LogErrorTime(split[0], openingErr, url);
        else if((openingErr & ~ignoreMask) != 0) Logger2.LogWarningTime(split[0], opening, openingErr, url, true);
        else Logger2.LogNormalTime(split[0], opening, url,true);

        if (closingErr.HasFlag(TimeParseError.UnabledToParse)) Logger2.LogErrorTime(split[1], closingErr, url);
        else if((closingErr & ~ignoreMask) != 0) Logger2.LogWarningTime(split[1], closing, closingErr, url, false);
        else Logger2.LogNormalTime(split[1], closing, url,false);


        return new OpeningHours(opening, closing);
    }

    private static (TimeOnly, TimeParseError) ParseOneSide(string side, List<OpeningSchedule> context, TimeOnly? firstSide = null, int depth = 0)
    {
        const int maxDepth = 4;

        var split = side.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        int hour, minute;
        if (split.Length == 2)
        {
            if (int.TryParse(split[0], out hour) && int.TryParse(split[1], out minute))
            {
                if ((!firstSide.HasValue || firstSide.Value.Hour < hour) && hour < 24 && minute < 60)
                {
                    if (hour == 0 && minute != 0)
                    {
                        if (depth > maxDepth)
                        {
                            return (new TimeOnly(hour, minute), TimeParseError.RecursionOverflow);
                        }
                        return ParseOneSide($"{split[1]}", context, firstSide, depth + 1);
                    }

                    return (new TimeOnly(hour, minute), TimeParseError.None);
                }

                if (firstSide.HasValue && firstSide.Value.Hour > hour)
                {
                    string rejoined = split[0] + split[1];
                    if (depth > maxDepth)
                    {
                        return (new TimeOnly(hour, minute),
                            TimeParseError.RecursionOverflow | TimeParseError.ClosingSoonerThanOpening);
                    }

                    return ParseOneSide(rejoined, context, firstSide, depth + 1);
                }

                if (firstSide.HasValue && hour == firstSide.Value.Hour && minute == firstSide.Value.Minute)
                {
                    if (hour == 0 && minute == 0)
                    {
                        hour = 23;
                        minute = 59;
                    }

                    var guess = GuessClosingFromContext(context);
                    if (guess.HasValue)
                    {
                        return (guess.Value, TimeParseError.None);
                    }

                    return (new TimeOnly(hour, minute), TimeParseError.None);
                }

                if (hour > 23)
                {
                    hour = 23;
                    minute = 59;
                    return (new TimeOnly(hour, minute), TimeParseError.OverflowHour);
                }

                if (minute > 59)
                {
                    if (hour == 0)
                    {
                        if (depth > maxDepth)
                        {
                            hour += minute / 60;
                            minute = minute % 60;

                            if (hour > 23)
                            {
                                hour = 23;
                                minute = 59;
                            }

                            return (new TimeOnly(hour, minute), TimeParseError.OverflowMinute | TimeParseError.RecursionOverflow);
                        }
                        return ParseOneSide($"{split[0]}{split[1]}", context, firstSide, depth + 1);
                    }

                    hour += minute / 60;
                    minute = minute % 60;

                    if (hour > 23)
                    {
                        hour = 23;
                        minute = 59;
                    }

                    return (new TimeOnly(hour, minute), TimeParseError.OverflowMinute);
                }
            }
            else
            {
                if (int.TryParse(split[0], out hour))
                {
                    minute = SusgeMinute;
                    return (new TimeOnly(hour, minute), TimeParseError.InvalidMinute);
                }

                if (int.TryParse(split[1], out minute))
                {
                    hour = SusgeHour;
                    return (new TimeOnly(hour, minute), TimeParseError.InvalidHour);
                }
            }
        }
        else if (split.Length != 1)
        {
            Console.WriteLine($"{side} | IsFirstSide:{firstSide is null}");
            throw new NotSupportedException("[Time parsing] One side has more than 2 elements");
        }
        else
        {
            if (int.TryParse(split[0], out hour))
            {
                if (hour > 23)
                {
                    if (split[0].Length > 2)
                    {
                        if (split[0].Length == 3)
                        {
                            hour = int.Parse(split[0].Substring(0, 1));
                            minute = int.Parse(split[0].Substring(1, 2));

                            if ((firstSide.HasValue && hour < firstSide.Value.Hour) || minute > 59)
                            {
                                var s1 = split[0].Substring(0, 2);
                                var s2 = split[0].Substring(2, 1);
                                if (depth > maxDepth)
                                {
                                    return (new TimeOnly(int.Parse(s1), int.Parse(s2)),
                                        TimeParseError.RecursionOverflow &
                                        TimeParseError.ClosingSoonerThanOpening &
                                        TimeParseError.SingleTag &
                                        TimeParseError.MoreThan2Digits);
                                }

                                return ParseOneSide($"{s1}:{s2}", context, firstSide, depth + 1);
                            }


                            return (new TimeOnly(hour, minute),
                                TimeParseError.SingleTag | TimeParseError.MoreThan2Digits);
                        }

                        if (split[0].Length == 4)
                        {
                            hour = int.Parse(split[0].Substring(0, 2));
                            minute = int.Parse(split[0].Substring(2, 2));
                            return (new TimeOnly(hour, minute),
                                TimeParseError.SingleTag | TimeParseError.MoreThan2Digits);
                        }

                        hour = SusgeHour;
                        minute = SusgeMinute;
                        return (new TimeOnly(hour, minute),
                            TimeParseError.SingleTag | TimeParseError.MoreThan2Digits | TimeParseError.Fallback &
                            TimeParseError.UnabledToParse);
                    }

                    hour = 23;
                    minute = 59;
                    return (new TimeOnly(hour, minute), TimeParseError.SingleTag | TimeParseError.OverflowHour);
                }

                return (new TimeOnly(hour, 0), TimeParseError.SingleTag);
            }
        }

        hour = SusgeHour;
        minute = SusgeMinute;
        return (new TimeOnly(hour, minute), TimeParseError.Fallback | TimeParseError.UnabledToParse);
    }

    private static TimeOnly? GuessClosingFromContext(List<OpeningSchedule> context)
    {
        if(context.Count == 0) return null;

        switch (context[^1].Day)
        {
            case "Hétfő":
            case "Kedd":
            case "Szerda":
            case "Csütörtök":
                for (var i = context.Count - 1; i >= 0; i--)
                {
                    var item = context[i];
                    if (!item.Closed)
                    {
                        return item.OpeningHours.Closing;
                    }
                }
                break;
            case "Péntek":
                break;
            case "Szombat":
                var last = context[^1];
                if (!last.Closed)
                {
                    return last.OpeningHours.Closing;
                }
                break;
            case "Vasárnap":
            default:
                throw new NotSupportedException("Nem jó nap");
        }

        return null;
    }

    [GeneratedRegex("^[zZ][áaAÁ][rR][vV][aA]$")]
    private static partial Regex IsClosedRegex();

    [GeneratedRegex("^.*[0-9]+.*$")]
    private static partial Regex ContainsNumber();
}