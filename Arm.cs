using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public class Arm : IUpdatable {
        public string Name { get; private set; }
        public UpdatableWrappedBlockHashSet<IMyTerminalBlock, Actuator> actuators;

        public string CurrentSequence { get; protected set; }
        public int CurrentSequenceStep { get; protected set; }


        public Arm(string name) {
            this.Name = name;
            this.CurrentSequence = null;

            actuators = new UpdatableWrappedBlockHashSet<IMyTerminalBlock, Actuator>(
                Program.Singleton.GridTerminalSystem,
                block => block.CustomName.Contains($"[{Program.TAG}_{Name}]"),
                block => ActuatorFactory.CreateActuator(block),
                wrapper => wrapper.Block);

        }

        public void Tick(IMyShipController currentController) {

            TickSequence();

            foreach (Actuator actuator in actuators) {
                if (actuator.Tick(currentController.MoveIndicator, currentController.RotationIndicator, currentController.RollIndicator, 1)) {
                    CurrentSequence = null;
                    CurrentSequenceStep = 0;
                }
            }
        }

        public void TickSequence() {

            if (CurrentSequence == null) {
                return;
            }

            bool anyActuatorStillMoving = false;
            foreach (Actuator actuator in actuators) {
                if (actuator.MovingToTarget) {
                    anyActuatorStillMoving = true;
                }
            }

            if (anyActuatorStillMoving) {
                return;
            }

            CurrentSequenceStep++;

            bool AnyActuatorHasNextStep = false;
            foreach (Actuator actuator in actuators) {
                if (actuator.ExectuteSequence(CurrentSequence, CurrentSequenceStep)) {
                    AnyActuatorHasNextStep = true;
                }
            }

            if (!AnyActuatorHasNextStep) {
                CurrentSequence = null;
                CurrentSequenceStep = 0;
            }
        }

        public void ExecuteSequence(string sequence) {
            this.CurrentSequence = sequence;
            this.CurrentSequenceStep = 0;
            foreach (Actuator actuator in actuators) {
                actuator.ExectuteSequence(CurrentSequence, CurrentSequenceStep);
            }
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            actuators.Update(description, currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateMultible(actuators, a => $"{Name} actuator {a.Block.CustomName}", currentStep, ref maxSteps, ref lastUpdateDescription);
        }

        public string DebugString() {
            string result = $"- {Name}";
            if (CurrentSequence != null) {
                result += $" seq: {CurrentSequence}, step: {CurrentSequenceStep}";
            }
            foreach (var item in actuators) {
                result += "\n" + item.DebugString();
            }

            return result;
        }
    }
}
