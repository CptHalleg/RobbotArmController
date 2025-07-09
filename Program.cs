using Sandbox.ModAPI.Ingame;
using System;
using System.Linq;

namespace IngameScript {
    public partial class Program : MyGridProgram {
        public const string TAG = "RAC";
        public const string TAG_BRACKETS = "[" + TAG + "]";
        public const string ARM_CUSTOM_DATA_TAG = "Arm:";
        public const float MAX_RADIAL_ACTUATOR_ERROR = 0.01f;
        public const float MAX_LINEAR_ACTUATOR_ERROR = 0.01f;
        public const UpdateFrequency UPDATE_FREQUENCY = UpdateFrequency.Update1;

        public static Program Singleton { get; private set; }

        private UpdatableWrappedBlockHashSet<IMyShipController, Controller> controllers;
        private UpdatableConfigDictionary<Arm> arms;


        public Program() {
            Logger.Log("Robbot Arm Controller Initializing...");
            Singleton = this;
            Runtime.UpdateFrequency = UPDATE_FREQUENCY;

            controllers = new UpdatableWrappedBlockHashSet<IMyShipController, Controller>(
                GridTerminalSystem,
                block => block.CustomName.Contains(TAG_BRACKETS),
                block => new Controller(block),
                wrapper => wrapper.Block);

            arms = new UpdatableConfigDictionary<Arm>(Me,
                section => section.StartsWith(ARM_CUSTOM_DATA_TAG),
                section => new Arm(section.Substring(ARM_CUSTOM_DATA_TAG.Length)));
            Updater.Init(Update);
            Logger.Init(this);
        }

        private void Update(int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            controllers.Update("controllers", currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateMultible(controllers, c => $"controller {c.Block.CustomName}", currentStep, ref maxSteps, ref lastUpdateDescription);
            arms.Update("arms", currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateMultible(arms.Values, a => $"arm {a.Name}", currentStep, ref maxSteps, ref lastUpdateDescription);
        }

        public void Save() {

        }

        public void Main(string argument, UpdateType updateSource) {
            ManageCommands(argument.Split(' '));
            if (Updater.Initialized) {
                Tick();
            } else {
                Updater.Tick();
                Logger.Tick();
            }
        }

        public void Tick() {
            Updater.Tick();
            Logic();
            Display();
            Logger.Tick();
        }

        private void Logic() {
            foreach (Controller controller in controllers) {
                if (controller.arm.Value != "" && arms.Keys.Contains(ARM_CUSTOM_DATA_TAG + controller.arm.Value)) {
                    arms[ARM_CUSTOM_DATA_TAG + controller.arm.Value].Tick(controller.Block);
                }
            }
        }

        private void ManageCommands(string[] arguments) {
            if (arguments.Length == 1 && arguments[0] == "test") {
                foreach (var arm in arms) {
                    foreach (var actuator in arm.Value.actuators) {
                        actuator.Test();
                    }
                }
            }
        }

        private void Display() {
            Logger.Display("Current Arms:");
            foreach (string name in arms.Keys) {
                Logger.Display($" - \"{arms[name].Name}\"");
                foreach (var actuator in arms[name].actuators) {
                    Logger.Display($"   - {actuator.Block.CustomName}");
                }
                Logger.Display("\n");
            }

            Logger.Display("Current Controllers:");
            foreach (var controller in controllers) {
                Logger.Display($" - {controller.Block.CustomName}, Arm: \"{controller.arm.Value}\"");
            }
        }
    }
}
