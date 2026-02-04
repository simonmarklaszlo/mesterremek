namespace SzivarClubManager.SourceGenerator.Factories.Target;

public record Factory(
    TypeName FactoryTypeName,
    TypeName ModelTypeName,
    bool DatabaseRequired)
{
    public string InstanceCreation(string? databaseConnectionVariableName)
    {
        if (DatabaseRequired) return $"new {FactoryTypeName.Name}({databaseConnectionVariableName})";
        return $"new {FactoryTypeName.Name}()";
    }
}