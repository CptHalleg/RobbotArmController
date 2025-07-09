using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    internal class PistonActuator : Actuator<IMyExtendedPistonBase> {
        public PistonActuator(IMyExtendedPistonBase block) : base(block) {
        }

        protected override float GetPosition() {
            return Actuated.CurrentPosition;
        }

        protected override void SetVelocity(float velocity) {
            Actuated.Velocity = velocity;
        }
    }
}
