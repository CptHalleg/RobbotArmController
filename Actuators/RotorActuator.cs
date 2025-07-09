
using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    internal class RotorActuator : Actuator<IMyMotorStator> {
        protected BoolValue autolock = new BoolValue("autolock", true);

        public RotorActuator(IMyMotorStator block) : base(block) {
            dataManager.AddDataValue(autolock);
        }

        protected override float GetPosition() {
            return Actuated.Angle;
        }

        protected override void SetVelocity(float velocity) {
            float targetVelocity = velocity;

            Actuated.RotorLock = autolock.Value && targetVelocity == 0;
            Actuated.TargetVelocityRad = targetVelocity;
        }
    }
}
