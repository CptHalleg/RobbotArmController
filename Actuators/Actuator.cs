using Sandbox.ModAPI.Ingame;
using System;
using VRageMath;

namespace IngameScript {
    public abstract class Actuator : TerminalBlockWrapper<IMyTerminalBlock>, IUpdatable {
        protected FloatValue leftRight = new FloatValue("left_right", 0f);
        protected FloatValue upDown = new FloatValue("up_down", 0f);
        protected FloatValue forwardBackward = new FloatValue("forward_backward", 0f);
        protected FloatValue yaw = new FloatValue("yaw", 0f);
        protected FloatValue pitch = new FloatValue("pitch", 0f);
        protected FloatValue roll = new FloatValue("roll", 0f);

        protected bool movingToTarget;
        protected float targetPosition;
        protected float targetSpeed;

        protected DataManager dataManager;

        public Actuator(IMyTerminalBlock block) : base(block) {
            dataManager = new ConfigDataManager(block, Program.TAG, leftRight, upDown, forwardBackward, yaw, pitch, roll);
            dataManager.LoadAll();
        }

        virtual protected void Update() {
            dataManager.LoadAll();
        }

        public void Update(string description, int currentStep, ref int maxSteps, ref string lastUpdateDescription) {
            if (Updater.ShouldUpdate(1, description, currentStep, ref maxSteps, ref lastUpdateDescription)) {
                Update();
            }
        }

        public virtual void Tick(Vector3 movement, Vector2 rotation, float roll, float multiplyer) {
            float velocity = 0;
            if (movingToTarget) {
                if (Math.Abs(GetPosition() - targetPosition) > 0.01f) {
                    velocity = MathHelper.Clamp(MathHelper.WrapAngle(targetPosition - GetPosition()) / 0.01f, -targetSpeed, targetSpeed);
                } else {
                    movingToTarget = false;
                    velocity = 0;
                }
            } else {
                velocity = CalculateMovement(movement, rotation, roll, multiplyer);
            }
            SetVelocity(velocity);
        }

        protected abstract void SetVelocity(float velocity);
        protected abstract float GetPosition();

        protected float CalculateMovement(Vector3 movement, Vector2 rotation, float rollRotation, float multiplyer) {
            float ret =
                  movement.X * leftRight.Value
                + movement.Y * upDown.Value
                + movement.Z * forwardBackward.Value
                + rotation.X * yaw.Value
                + rotation.Y * pitch.Value
                + rollRotation * roll.Value;
            ret *= multiplyer;
            return ret;
        }

        internal void Test() {
            movingToTarget = true;
            targetPosition = 0;
            targetSpeed = 0.5f;
        }
    }
}
