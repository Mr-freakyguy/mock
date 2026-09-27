using CoffeeMachine.Enum;

namespace CoffeeMachine.Logger
{
    public interface ILogger
    {
        Task WriteAsync(LogLevel level, string message);
        Task<IEnumerable<string>> ReadAllAsync();
    }
}
