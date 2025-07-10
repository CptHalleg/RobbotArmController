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
            if (arguments.Length == 3 && arguments[0] == "sequence") {
                string armName = arguments[1];
                if (!arms.ContainsKey(armName)) {
                    Logger.Error($"Arm \"{armName}\" not found.");
                    return;
                }
                Arm arm = arms[armName];
                string sequenceName = arguments[2];
                arm.ExecuteSequence(sequenceName);
                Logger.Log($"Executing sequence \"{sequenceName}\" on arm \"{arm.Name}\".");

            }
        }

        private void Display() {
            string display = "";
            display += "Current Arms:";

            foreach (string name in arms.Keys) {
                display += "\n" + arms[name].DebugString();
            }

            display += "Current Controllers:";
            foreach (var controller in controllers) {
                display += $"\n - {controller.Block.CustomName}, Arm: \"{controller.arm.Value}\"";
            }
            Logger.Display(display);
        }
    }
}
