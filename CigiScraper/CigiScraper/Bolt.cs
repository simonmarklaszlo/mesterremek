namespace CigiScraper;

public class Bolt
{
    public double Longitude { get; init;}
    public double Latitude { get; init;}
    public string Location { get; init;}
    public string Street { get; init;}
    public Dictionary<string,Nyitvatartas> Nyitvatartasok { get; init;}
}

public record Nyitvatartas(TimeOnly Nyitas, TimeOnly Zaras);