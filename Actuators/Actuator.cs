using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript {
    public abstract class Actuator : TerminalBlockWrapper<IMyTerminalBlock>, IUpdatable {
        protected DataValue<float> leftRight = new DataValue<float>("left_right", 0f, new FloatConverter());
        protected DataValue<float> upDown = new DataValue<float>("up_down", 0f, new FloatConverter());
        protected DataValue<float> forwardBackward = new DataValue<float>("forward_backward", 0f, new FloatConverter());
        protected DataValue<float> yaw = new DataValue<float>("yaw", 0f, new FloatConverter());
        protected DataValue<float> pitch = new DataValue<float>("pitch", 0f, new FloatConverter());
        protected DataValue<float> roll = new DataValue<float>("roll", 0f, new FloatConverter());

        protected DataValue<Dictionary<string, Dictionary<int, Touple<float, float>>>> sequences =
            new DataValue<Dictionary<string, Dictionary<int, Touple<float, float>>>>("sequences", new Dictionary<string, Dictionary<int, Touple<float, float>>>(),
                new DictionaryConverter<string, Dictionary<int, Touple<float, float>>>(
                    new StringConverter(),
                    new DictionaryConverter<int, Touple<float, float>>(
                        new IntConverter(),
                        new ToupleConverter<float, float>(
                            new FloatConverter(),
                            new FloatConverter(),
                            ';'),
                        '|',
                        ':'),
                    ',',
                    '='));


        public bool MovingToTarget { get; protected set; }
        public float TargetPosition { get; protected set; }
        public float TargetSpeed { get; protected set; }

        protected DataManager dataManager;

        public Actuator(IMyTerminalBlock block) : base(block) {
            dataManager = new ConfigDataManager(block, Program.TAG, leftRight, upDown, forwardBackward, yaw, pitch, roll, sequences);
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

        public virtual bool Tick(Vector3 movement, Vector2 rotation, float roll, float multiplyer) {
            float velocity = 0;
            bool manualMovement = false;
            if (MovingToTarget) {
                if (Math.Abs(GetPosition() - GetTargetPositionRad()) > 0.01f) {
                    velocity = MathHelper.Clamp(MathHelper.WrapAngle(GetTargetPositionRad() - GetPosition()) / 0.01f, -GetTargetSpeedRad(), GetTargetSpeedRad());
                } else {
                    MovingToTarget = false;
                    velocity = 0;
                }
            } else {
                velocity = CalculateMovement(movement, rotation, roll, multiplyer);
                if (velocity != 0) {
                    manualMovement = true;
                }
            }
            SetVelocity(velocity);
            return manualMovement;
        }

        public float GetTargetPositionRad() {
            return MathHelper.ToRadians(TargetPosition);
        }
        public float GetTargetSpeedRad() {
            return MathHelper.ToRadians(TargetSpeed);
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

        public void MoveTo(float targetPosition, float targetSpeed) {
            Logger.Log($"Actuator {Block.CustomName} moving to target position {targetPosition} with speed {targetSpeed}.");
            MovingToTarget = true;
            this.TargetPosition = targetPosition;
            this.TargetSpeed = targetSpeed;
        }

        public bool ExectuteSequence(string sequenceName, int step) {
            if (!sequences.Value.ContainsKey(sequenceName)) {
                return false;
            }
            Dictionary<int, Touple<float, float>> sequence = sequences.Value[sequenceName];
            if (!sequence.ContainsKey(step)) {
                return false;
            }
            Touple<float, float> data = sequence[step];
            MoveTo(data.Item1, data.Item2);
            return true;
        }

        public string DebugString() {
            string result = $"   - {Block.CustomName}";

            foreach (var seq in sequences.Value) {
                result += $"\n     {seq.Key}:";
                foreach (var prio in seq.Value) {
                    result += $"\n          {prio.Key}: {prio.Value.Item1},{prio.Value.Item2}";
                }
            }
            return result;
        }
    }
}
