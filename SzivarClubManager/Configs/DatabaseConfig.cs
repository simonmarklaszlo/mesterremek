using DotNetEnv;

namespace SzivarClubManager.Configs;

public record DatabaseConfig(string Host, int Port, string User, string Password, string DatabaseName)
{
    public static DatabaseConfig Load()
    {
        Env.Load();
        return new DatabaseConfig(
            Env.GetString("DB_HOST"),
            Env.GetInt("DB_PORT", 5432),
            Env.GetString("DB_USER"),
            Env.GetString("DB_PASSWORD"),
            Env.GetString("DB_DATABASE", "szivarclub"));
    }
}