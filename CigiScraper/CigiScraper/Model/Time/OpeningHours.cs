namespace CigiScraper.Model.Time;

public class OpeningHours
{
    public string Opening { get; }
    public string Closing { get; }

    public OpeningHours(string opening, string closing)
    {
        Opening = opening;
        Closing = closing;
    }

    public override string ToString()
    {
        return $"{Opening} - {Closing}";
    }
}