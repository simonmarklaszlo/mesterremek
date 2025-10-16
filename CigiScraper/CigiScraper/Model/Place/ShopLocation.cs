namespace CigiScraper.Model.Place;

public class ShopLocation
{
    public int PostalCode { get; }
    public string City { get; }
    public string Address { get; }

    public ShopLocation(int postalCode, string city, string address)
    {
        PostalCode = postalCode;
        City = city;
        Address = address;
    }

    public override string ToString()
    {
        return $"{PostalCode}, {City}, {Address}";
    }

}