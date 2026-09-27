using CoffeeMachine.Enum;
using System.Text;

namespace CoffeeMachine.Logger
{
    public class CsvLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public CsvLogger(string fileName = "CoffeeMachineLog.txt")
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        }

        public async Task WriteAsync(LogLevel level, string message)
        {
            await _lock.WaitAsync();
            try
            {
                bool fileExists = File.Exists(_filePath);
                using FileStream stream = new(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true);

                if (!fileExists)
                {
                    byte[] header = Encoding.UTF8.GetBytes("Timestamp,Level,Message" + Environment.NewLine);
                    await stream.WriteAsync(header);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                // Edge case: message field isn't escaped, so a message containing a comma or quote will misalign columns
                string line = $"[{timestamp}],[{level}],'{message}'{Environment.NewLine}";
                byte[] bytes = Encoding.UTF8.GetBytes(line);
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
