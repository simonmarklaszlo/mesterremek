namespace SzivarClubManager.Models;

public record Cigar(
    int Id,
    string Name,
    Brand Brand)
{
    public string ToCopiableString() => $"{Id} {Name} {Brand}";
    public Cigar Copy() => this with { Brand = Brand.Copy() };
}