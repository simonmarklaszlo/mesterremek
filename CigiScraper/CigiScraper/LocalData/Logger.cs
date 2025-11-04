using System.Text;

namespace CigiScraper.LocalData;

public sealed class Logger : IAsyncDisposable
{
    public const string DirectoryPath = "./archive/logs/";
    private readonly string _baseDirPath;
    private readonly string _logName;
    private string LogFileName => _logName.EndsWith(".txt") ? _logName : _logName + ".txt";
    private string LogFilePath => Path.Combine(_baseDirPath, LogFileName);
    private readonly List<string> _logs;
    private const int MaxLogs = 100;

    public Logger(string logName, string baseDirPath = DirectoryPath)
    {
        _logName = logName;
        _baseDirPath = baseDirPath;
        _logs = new List<string>(MaxLogs);

        Directory.CreateDirectory(_baseDirPath);
        var files = Directory.GetFiles(_baseDirPath);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    public Task Log(string message) => RecordMessage(message);
    public Task Warn(string message) => RecordMessage(message);
    public Task Error(string message) => RecordMessage(message);

    private Task RecordMessage(string message)
    {
        _logs.Add(message);

        if (_logs.Count != MaxLogs) return Task.CompletedTask;

        return Flush();
    }

    private async Task Flush()
    {
        await using var writer = new StreamWriter(LogFilePath, true);
        foreach (var log in _logs)
        {
            await writer.WriteLineAsync(log);
        }

        _logs.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_logs.Count > 0)
        {
            await Flush();
        }
    }
}