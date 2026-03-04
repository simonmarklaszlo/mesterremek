namespace SzivarClubManager.Models;

public class Cigar : IModel
{
    public virtual int Id { get; }
    public string Name { get; protected set; }
    public Brand Brand { get; protected set; }

    public Cigar(int id, string name, Brand brand)
    {
        Id = id;
        Name = name;
        Brand = brand;
    }

    public string ToCopiableString() => $"{Id} {Name} {Brand}";
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is Cigar cigar && cigar.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
    public static Cigar CreateNew(string name, Brand brand) => new(-1, name, brand);
}