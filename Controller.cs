using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public class Controller : TerminalBlockWrapper<IMyShipController>, IUpdatable {
        protected ConfigManager configManager;
        public ConfigValue<string> arm = new ConfigValue<string>("arm", "", new StringConverter());


        public Controller(IMyShipController myShipController) : base(myShipController) {
            configManager = new ConfigManager(Block, Program.TAG, arm);
        }

        private void Update() {
            configManager.LoadAll();
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            if (Updater.ShouldUpdate(1, description, currentStep, ref maxSteps, ref lastUpdateDescription)) {
                Update();
            }
        }

        internal object DebugString() {
            return $" - {Block.CustomName}, Arm{arm.Name}";
        }
    }
}
