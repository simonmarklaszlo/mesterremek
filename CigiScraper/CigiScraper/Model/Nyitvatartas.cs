namespace CigiScraper.Model;

public class Nyitvatartas
{
    public bool Closed { get; }
    public string Day { get; }
    public Idotartam? Idotartam { get; }

    public Nyitvatartas(string day, Idotartam? idotartam = null)
    {
        Closed = idotartam is null;
        Day = day;
        Idotartam = idotartam;
    }

    public static string ToCsvLine(Nyitvatartas[] nyitvatartasok)
    {
        var str = "";
        foreach (var nyitvatartas in nyitvatartasok)
        {
            if (nyitvatartas.Closed)
            {
                str += $"{nyitvatartas.Day}-Zárva|";
            }
            else
            {
                str += $"{nyitvatartas.Day}-{nyitvatartas.Idotartam!.Opening}:{nyitvatartas.Idotartam.Closing}|";
            }
        }
        return str.TrimEnd('|');
    }
    public override string ToString()
    {
        return $"{Day} - {(Closed ? "Zárva" : Idotartam)}";
    }

    public static bool NyitvatartasokEqual(Nyitvatartas[] a, Nyitvatartas[] b)
    {
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Nyitvatartas ny) return false;

        if(Day != ny.Day) return false;
        if(Closed != ny.Closed) return false;
        if(Idotartam != ny.Idotartam) return false;

        return true;
    }

    protected bool Equals(Nyitvatartas other)
    {
        return Closed == other.Closed && Day == other.Day && Equals(Idotartam, other.Idotartam);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Closed, Day, Idotartam);
    }
}