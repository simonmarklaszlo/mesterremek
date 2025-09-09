namespace CigiScraper.Model;

public class Idotartam
{
    public string Opening { get; }
    public string Closing { get; }

    public Idotartam(string opening, string closing)
    {
        Opening = opening;
        Closing = closing;
    }

    public override string ToString()
    {
        return $"{Opening} - {Closing}";
    }

    public override bool Equals(object? obj)
    {
        if(obj is not Idotartam idotartam) return false;
        return Opening == idotartam.Opening && Closing == idotartam.Closing;
    }

    protected bool Equals(Idotartam other)
    {
        return Opening == other.Opening && Closing == other.Closing;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Opening, Closing);
    }
}