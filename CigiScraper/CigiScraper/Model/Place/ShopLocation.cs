namespace CigiScraper.Model.Place;

public class ShopLocation
{
    public int PostalCode { get; }
    public string LocationName { get; }
    public string Address { get; }

    public ShopLocation(int postalCode, string locationName, string address)
    {
        PostalCode = postalCode;
        LocationName = locationName;
        Address = address;
    }

    public override string ToString()
    {
        return $"{PostalCode}, {LocationName}, {Address}";
    }

}