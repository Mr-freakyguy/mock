using CoffeeMachine.Enum;

namespace CoffeeMachine.Model
{
    public class Machine
    {
        public event Action<MachineStatus>? StatusChanged;
        public event Action<LidStatus>? LidChanged;

        private MachineStatus _systemStatus;
        public MachineStatus SystemStatus
        {
            get => _systemStatus;
            set
            {
                _systemStatus = value;
                StatusChanged?.Invoke(value);
            }
        }

        private LidStatus _lidStatus;
        public LidStatus LidStatus
        {
            get => _lidStatus;
            set
            {
                _lidStatus = value;
                LidChanged?.Invoke(value);
            }
        }
    }
}
