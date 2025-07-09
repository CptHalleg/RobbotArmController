using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public class Arm : IUpdatable {
        public string Name { get; private set; }
        public UpdatableWrappedBlockHashSet<IMyTerminalBlock, Actuator> actuators;


        public Arm(string name) {
            this.Name = name;

            actuators = new UpdatableWrappedBlockHashSet<IMyTerminalBlock, Actuator>(
                Program.Singleton.GridTerminalSystem,
                block => block.CustomName.Contains($"[{Program.TAG}_{Name}]"),
                block => ActuatorFactory.CreateActuator(block),
                wrapper => wrapper.Block);

        }

        public void Tick(IMyShipController currentController) {

            foreach (Actuator actuator in actuators) {
                actuator.Tick(currentController.MoveIndicator, currentController.RotationIndicator, currentController.RollIndicator, 1);
            }
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            actuators.Update(description, currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateMultible(actuators, a => $"arm {Name} actuator", currentStep, ref maxSteps, ref lastUpdateDescription);
        }
    }
}
