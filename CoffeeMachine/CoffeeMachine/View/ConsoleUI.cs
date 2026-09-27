using CoffeeMachine.Service;

namespace CoffeeMachine.View
{
    public class ConsoleUI
    {
        private const int HeaderRow = 0;
        private const int HeaderHeight = 4;
        private const int MidRow = HeaderHeight + 1;
        private const int MidHeight = 14;
        private const int FooterRow = MidRow + MidHeight + 1;
        private const int FooterHeight = 3;

        private readonly MachineService _service;
        private int _secondsRemaining;

        public ConsoleUI(MachineService service)
        {
            _service = service;
            _service._Machine.StatusChanged += _ => { _secondsRemaining = 0; RewriteHeader(); };
            _service._Machine.LidChanged += _ => RewriteHeader();
            _service.CountdownTick += (_, seconds) => { _secondsRemaining = seconds; RewriteHeader(); };
            _service.NotificationRaised += RewriteFooter;
        }

        public async void Run()
        {
            await _service.InitializeAsync();
            RewriteHeader();
            RewriteFooter("System ready.");

            bool running = true;
            while (running)
            {
                ShowMenu();
                int choice = ConsoleHelper.GetIntegerInput("", 1, 7);

                switch (choice)
                {
                    case 1: _ = _service.StartBrewSequenceAsync(); break;
                    case 2: await _service.StopBrewSequenceAsync(); break;
                    case 3: await _service.SimulateErrorAsync(); break;
                    case 4: await _service.ToggleLidAsync(); break;
                    case 5: await _service.ResetMachineAsync(); break;
                    case 6:
                        await ShowLogAsync();
                        ConsoleHelper.GetNonEmptyString("Press enter to return to menu...");
                        break;
                    case 7: running = false; break;
                }
            }
        }
        public void RewriteHeader()
        {
            ClearRegion(HeaderRow, HeaderHeight);
            Console.SetCursorPosition(0, HeaderRow);
            Console.WriteLine("==================== Coffee Machine ====================");
            Console.SetCursorPosition(0, HeaderRow + 1);
            Console.WriteLine($"State: {_service._Machine.SystemStatus}   Lid: {_service._Machine.LidStatus}");
            Console.SetCursorPosition(0, HeaderRow + 2);
            Console.WriteLine($"Time remaining in state: {_secondsRemaining}s");
            Console.SetCursorPosition(0, HeaderRow + 3);
            Console.WriteLine("==========================================================");
        }

        public void RewriteMid(IEnumerable<string> lines)
        {
            ClearRegion(MidRow, MidHeight);
            int row = MidRow;
            foreach (var line in lines)
            {
                // Edge case: content longer than MidHeight lines is truncated, not scrolled
                if (row >= MidRow + MidHeight) break;
                Console.SetCursorPosition(0, row);
                Console.WriteLine(line);
                row++;
            }
        }

        public void RewriteFooter(string message)
        {
            ClearRegion(FooterRow, FooterHeight);
            Console.SetCursorPosition(0, FooterRow);
            Console.WriteLine("---------------------------------------------------------");
            Console.SetCursorPosition(0, FooterRow + 1);
            Console.WriteLine($"Notice: {message}");
        }

        public void ShowMenu()
        {
            RewriteMid(new[]
            {
                "1. Start brew sequence",
                "2. Stop brew sequence",
                "3. Simulate machine error",
                "4. Toggle lid",
                "5. Reset machine",
                "6. View event log",
                "7. Exit",
                "",
                "Select an option: "
            });
        }

        public async Task ShowLogAsync()
        {
            var lines = await _service.ReadLogLinesAsync();
            RewriteMid(lines);
        }

        private void ClearRegion(int startRow, int height)
        {
            string blank = new string(' ', Console.WindowWidth - 1);
            for (int i = 0; i < height; i++)
            {
                Console.SetCursorPosition(0, startRow + i);
                Console.Write(blank);
            }
        }
    }
}
