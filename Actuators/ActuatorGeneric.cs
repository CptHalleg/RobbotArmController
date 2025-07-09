using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public abstract class Actuator<T> : Actuator where T : class, IMyTerminalBlock {
        public T Actuated { get { return Block as T; } }

        protected Actuator(T block) : base(block) {
        }
    }
}
