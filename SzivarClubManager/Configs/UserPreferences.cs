namespace SzivarClubManager.Configs;

public record UserPreferences(bool UseLightTheme, int PageSize)
{
    public static UserPreferences Load() => new(true, 20);
}