namespace SzivarClubManager.Configs;

public class GlobalConfig
{
    public DatabaseConfig DatabaseConfig { get; } = DatabaseConfig.Load();
    public UserPreferences UserPreferences { get; } = UserPreferences.Load();

    public static GlobalConfig Instance { get; } = new();
}