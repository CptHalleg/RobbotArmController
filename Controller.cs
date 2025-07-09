using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public class Controller : TerminalBlockWrapper<IMyShipController>, IUpdatable {
        protected ConfigDataManager dataManager;
        public DataValue<string> arm = new DataValue<string>("arm", "", new StringConverter(), saveAfterWrite: true);


        public Controller(IMyShipController myShipController) : base(myShipController) {
            dataManager = new ConfigDataManager(Block, Program.TAG, arm);
        }

        private void Update() {
            dataManager.LoadAll();
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            if (Updater.ShouldUpdate(1, description, currentStep, ref maxSteps, ref lastUpdateDescription)) {
                Update();
            }
        }
    }
}
