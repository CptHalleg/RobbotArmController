using Sandbox.ModAPI.Ingame;
using System;
using VRageMath;

namespace IngameScript {
    public abstract class Actuator : TerminalBlockWrapper<IMyTerminalBlock>, IUpdatable {
        protected DataValue<float> leftRight = new DataValue<float>("left_right", 0f, new FloatConverter());
        protected DataValue<float> upDown = new DataValue<float>("up_down", 0f, new FloatConverter());
        protected DataValue<float> forwardBackward = new DataValue<float>("forward_backward", 0f, new FloatConverter());
        protected DataValue<float> yaw = new DataValue<float>("yaw", 0f, new FloatConverter());
        protected DataValue<float> pitch = new DataValue<float>("pitch", 0f, new FloatConverter());
        protected DataValue<float> roll = new DataValue<float>("roll", 0f, new FloatConverter());

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
