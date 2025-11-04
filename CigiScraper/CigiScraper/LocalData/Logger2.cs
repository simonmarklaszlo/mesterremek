using CigiScraper.Model.Time;

namespace CigiScraper.LocalData;

public static class Logger2
{
    private const string TimeErrorPath = "archive/time/error.csv";
    private const string TimeWarningPath = "archive/time/warning.csv";
    private const string TimeNormalPath = "archive/time/normal.csv";

    private static readonly List<string> TimeErrors = [];
    private static readonly List<string> TimeWarnings = [];
    private static readonly List<string> TimeNormal = [];

    static Logger2()
    {
        if (!Directory.Exists("archive/time"))
        {
            Directory.CreateDirectory("archive/time");
        }
    }

    public static void LogErrorTime(string input, TimeParseError error, string url)
    {
        TimeErrors.Add($"{input}|{error:G}|{url}");
    }

    public static void LogWarningTime(string input, TimeOnly time, TimeParseError error, string url, bool isOpening)
    {
        TimeWarnings.Add($"{(isOpening ? "OP" : "CL")}|{input}|{time:HH:mm}|{error:G}|{url}");
    }

    public static void LogNormalTime(string input, TimeOnly time, string url, bool isOpening)
    {
        TimeNormal.Add($"{(isOpening ? "OP" : "CL")}|{input}|{time:HH:mm}|{url}");
    }
    public static async Task Flush()
    {
        await File.WriteAllLinesAsync(TimeErrorPath, TimeErrors);
        await File.WriteAllLinesAsync(TimeWarningPath, TimeWarnings);
        await File.WriteAllLinesAsync(TimeNormalPath, TimeNormal);

        TimeErrors.Clear();
        TimeWarnings.Clear();
        TimeNormal.Clear();
    }
}