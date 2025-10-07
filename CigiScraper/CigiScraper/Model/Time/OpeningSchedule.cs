namespace CigiScraper.Model.Time;

public class OpeningSchedule
{
    public bool Closed { get; }
    public string Day { get; }
    public OpeningHours? OpeningHours { get; }

    public OpeningSchedule(string day, OpeningHours? openingHours = null)
    {
        Closed = openingHours is null;
        Day = day;
        OpeningHours = openingHours;
    }

    public static string ToCsvLine(OpeningSchedule[] openingSchedules)
    {
        var str = "";
        foreach (var op in openingSchedules)
        {
            if (!op.Closed)
            {
                str += $"{op.Day} {op.OpeningHours!.Opening}-{op.OpeningHours.Closing}|";
            }
        }
        return str.TrimEnd('|');
    }
    public override string ToString()
    {
        return $"{Day} - {(Closed ? "Zárva" : OpeningHours)}";
    }
}