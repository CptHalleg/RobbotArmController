using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript {
    public class Arm : IUpdatable {
        public string Name { get; private set; }
        protected BlockDictionary<IMyTerminalBlock, Actuator> actuators;
        protected BlockReference<IMyTerminalBlock> root;
        protected BlockReference<IMyTerminalBlock> tip;
        protected ConfigManager configManager;

        protected string CurrentSequence;
        protected int CurrentSequenceStep;
        protected Vector3 targetPosition;

        public Arm(string name) {
            this.Name = name;
            this.CurrentSequence = null;

            actuators = new BlockDictionary<IMyTerminalBlock, Actuator>(
                Program.Singleton.GridTerminalSystem,
                block => block.CustomName.Contains($"[{Program.TAG}_{Name}]"),
                block => ActuatorFactory.CreateActuator(block));

            root = new BlockReference<IMyTerminalBlock>(Program.Singleton.GridTerminalSystem, b => b.CustomName.Contains("[root]"));
            tip = new BlockReference<IMyTerminalBlock>(Program.Singleton.GridTerminalSystem, b => b.CustomName.Contains("[tip]"));

        }

        public void Tick(IMyShipController currentController) {

            TickSequence();

            targetPosition += currentController.MoveIndicator * 0.01f;

            Logger.Display("target pos: " + targetPosition);
            foreach (var pair in actuators.CleanedItems()) {
                Actuator actuator = pair.Value;
                if (actuator.Tick(currentController.MoveIndicator, currentController.RotationIndicator, currentController.RollIndicator, 1, targetPosition)) {
                    CurrentSequence = null;
                    CurrentSequenceStep = 0;
                }
            }
        }

        protected Vector3 NewPos(IMyShipController currentController) {
            return tip.Block.GetPosition() + Vector3D.Transform(currentController.MoveIndicator, MatrixD.Invert(root.Block.WorldMatrix));
        }

        public void TickSequence() {

            if (CurrentSequence == null) {
                return;
            }

            bool anyActuatorStillMoving = false;
            foreach (var pair in actuators.CleanedItems()) {
                Actuator actuator = pair.Value;
                if (actuator.MovingToTarget) {
                    anyActuatorStillMoving = true;
                }
            }

            if (anyActuatorStillMoving) {
                return;
            }

            CurrentSequenceStep++;

            bool AnyActuatorHasNextStep = false;
            foreach (var pair in actuators.CleanedItems()) {
                Actuator actuator = pair.Value;
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
            foreach (var pair in actuators.CleanedItems()) {
                Actuator actuator = pair.Value;
                actuator.ExectuteSequence(CurrentSequence, CurrentSequenceStep);
            }
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            actuators.Update("actuators", currentStep, ref maxSteps, ref lastUpdateDescription);
            root.Update("root", currentStep, ref maxSteps, ref lastUpdateDescription);
            tip.Update("tip", currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateCollection(actuators.CleanedValues(), a => $"{Name} actuator {a.Block.CustomName}", currentStep, ref maxSteps, ref lastUpdateDescription);
        }

        public string DebugString() {
            string result = $"- {Name}";
            if (CurrentSequence != null) {
                result += $"\n seq: {CurrentSequence}, step: {CurrentSequenceStep}";
            }
            foreach (var item in actuators.CleanedValues()) {
                result += "\n" + item.DebugString();
            }

            return result;
        }
    }
}
