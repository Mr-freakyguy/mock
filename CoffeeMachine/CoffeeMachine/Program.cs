using CoffeeMachine.Logger;
using CoffeeMachine.Model;
using CoffeeMachine.Service;
using CoffeeMachine.View;

namespace CoffeeMachine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.WriteLine("Coffee Machine Controller Initialized");
            await Task.Delay(1000);

            var machine = new Machine();
            var logger = new CsvLogger();
            var service = new MachineService(machine, logger);
            var ui = new ConsoleUI(service);

            ui.Run();
        }
    }
}
