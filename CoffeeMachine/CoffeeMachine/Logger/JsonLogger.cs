using CoffeeMachine.Enum;
using System.Text;
using System.Text.Json;

namespace CoffeeMachine.Logger
{
    public class JsonLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public JsonLogger(string fileName = "CoffeeMachineLog.json")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        }

        public async Task WriteAsync(LogLevel level, string message)
        {
            await _lock.WaitAsync();
            try
            {
                var entry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Level = level.ToString(),
                    Message = message
                };
                // Edge case: each call appends one JSON object per line (NDJSON), not a single valid JSON array
                string json = JsonSerializer.Serialize(entry) + Environment.NewLine;
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using FileStream stream = new(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true);
                await stream.WriteAsync(bytes);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IEnumerable<string>> ReadAllAsync()
        {
            if (!File.Exists(_filePath)) return new[] { "No log entries yet." };
            using FileStream stream = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
            using StreamReader reader = new(stream);
            string content = await reader.ReadToEndAsync();
            return content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
