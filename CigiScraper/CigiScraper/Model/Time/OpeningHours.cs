namespace CigiScraper.Model.Time;

public class OpeningHours
{
    public TimeOnly Opening { get; }
    public TimeOnly Closing { get; }

    public OpeningHours(string opening, string closing)
    {
        var o= GetTime(opening);
        var c = GetTime(closing);

        (Opening, Closing) = FixTime((o, c));
    }

    private static TimeOnly GetTime(string time)
    {
        if(time == "0:70")return new TimeOnly(7, 0);

        var split = time.Split(':');

        int hoursInt, minutesInt;

        if (split.Length == 2)
        {
            hoursInt = int.Parse(split[0]);
            minutesInt = int.Parse(split[1]);
        }
        else
        {
            hoursInt = int.Parse(split[0]);
            minutesInt = 0;
        }

        int minutes = minutesInt % 60;
        int hours = hoursInt + minutesInt / 60;

        if (hours > 23)
        {
            hours = 23;
            minutes = 59;
        }

        return new TimeOnly(hours, minutes);
    }

    private static (TimeOnly,TimeOnly)FixTime((TimeOnly,TimeOnly) times)
    {
        if (times.Item1.Hour == 0 && times.Item2.Hour == 0)
        {
            return (GetTime($"{times.Item1.Minute:00}:00"),GetTime($"{times.Item2.Minute:00}:00"));
        }

        return times;
    }
    public override string ToString()
    {
        return $"{Opening} - {Closing}";
    }
}