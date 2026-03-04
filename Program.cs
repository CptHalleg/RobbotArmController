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

        private SEPB sepb;
        private BlockDictionary<IMyShipController, Controller> controllers;
        private ConfigSectionDictionary<Arm> arms;
        private ConfigManager configManager;


        public Program() {
            Singleton = this;
            sepb = new SEPB(this, UPDATE_FREQUENCY, Logic, Display, Update);
            Logger.Log("Robbot Arm Controller Initializing...");

            controllers = new BlockDictionary<IMyShipController, Controller>(GridTerminalSystem,
                block => block.CustomName.Contains(TAG_BRACKETS),
                block => new Controller(block));

            arms = new ConfigSectionDictionary<Arm>(
                section => section.StartsWith(ARM_CUSTOM_DATA_TAG),
                section => new Arm(section.Substring(ARM_CUSTOM_DATA_TAG.Length)));

            configManager = new ConfigManager(Me, ARM_CUSTOM_DATA_TAG, arms);

            Command<string> sequenceCommand = new Command<string>((x) => { }, new StringConverter());
        }

        private void Update(int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            controllers.Update("controllers", currentStep, ref maxSteps, ref lastUpdateDescription);
            Updater.UpdateCollection(controllers.CleanedValues(), c => $"controller {c.Block.CustomName}", currentStep, ref maxSteps, ref lastUpdateDescription);
            if (Updater.ShouldUpdate(1, "arms", currentStep, ref maxSteps, ref lastUpdateDescription)) {
                configManager.LoadAll();
            }
            Updater.UpdateCollection(arms.CleanedValues(), a => $"arm {a.Name}", currentStep, ref maxSteps, ref lastUpdateDescription);
        }

        public void Save() {

        }

        public void Main(string argument, UpdateType updateSource) {
            sepb.Main(argument, updateSource);
        }

        private void Logic() {
            foreach (Controller controller in controllers.CleanedValues()) {
                if (controller.arm.Value != "" && arms.Items.Keys.Contains(ARM_CUSTOM_DATA_TAG + controller.arm.Value)) {
                    arms.Items[ARM_CUSTOM_DATA_TAG + controller.arm.Value].Tick(controller.Block);
                }
            }
        }

        private void Display() {
            string display = "";
            display += "Current Arms:";

            foreach (Arm arm in arms.CleanedValues()) {
                display += $"\n{arm.DebugString()}";
            }

            display += "\nCurrent Controllers:";
            foreach (var controller in controllers.CleanedValues()) {
                display += $"\n{controller.DebugString()}";
            }
            Logger.Display(display);
        }
    }
}
