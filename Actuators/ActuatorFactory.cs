using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public class ActuatorFactory {
        public static Actuator CreateActuator(IMyTerminalBlock block) {

            if (block is IMyMotorStator) {
                return new RotorActuator(block as IMyMotorStator);
            } else if (block is IMyExtendedPistonBase) {
                return new PistonActuator(block as IMyExtendedPistonBase);
            }


            return null;
        }
    }
}
