namespace SzivarClubManager.SourceGenerator.Factories.Target;

public record FakeFactory(
    TypeName FactoryTypeName,
    TypeName ModelTypeName
) : Factory(FactoryTypeName, ModelTypeName, false)
{
    public string InstanceCreation() => $"new {FactoryTypeName.Name}()";
}