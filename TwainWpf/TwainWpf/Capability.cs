using System;
using TwainWpf.TwainNative;

namespace TwainWpf
{
    public class Capability
    {
        private readonly Identity _applicationId;
        private readonly Identity _sourceId;
        private readonly Capabilities _capability;
        private readonly TwainType _twainType;

        public Capability(Capabilities capability, TwainType twainType, Identity applicationId, Identity sourceId)
        {
            _capability = capability;
            _applicationId = applicationId;
            _sourceId = sourceId;
            _twainType = twainType;
        }

        public BasicCapabilityResult GetBasicValue()
        {
            var oneValue = new CapabilityOneValue(_twainType, 0);
            var twainCapability = TwainCapability.From(_capability, oneValue);

            var result = Twain32Native.DsCapability(
                    _applicationId,
                    _sourceId,
                    DataGroup.Control,
                    DataArgumentType.Capability,
                    Message.Get,
                    twainCapability);

            if (result != TwainResult.Success)
            {
                var conditionCode = GetStatus();

                return new BasicCapabilityResult()
                {
                    ConditionCode = conditionCode,
                    ErrorCode = result
                };
            }

            twainCapability.ReadBackValue();

            return new BasicCapabilityResult()
            {
                RawBasicValue = oneValue.Value
            };
        }

        protected ConditionCode GetStatus()
        {
            return DataSourceManager.GetConditionCode(_applicationId, _sourceId);
        }

        public void SetValue(short value)
        {
            SetValue<short>(value);
        }

        protected void SetValue<T>(T value)
        {
            var rawValue = Convert.ToInt32(value);
            var oneValue = new CapabilityOneValue(_twainType, rawValue);
            var twainCapability = TwainCapability.From(_capability, oneValue);

            var result = Twain32Native.DsCapability(
                _applicationId,
                _sourceId,
                DataGroup.Control,
                DataArgumentType.Capability,
                Message.Set,
                twainCapability);

            if (result == TwainResult.Success)
            {
                return;
            }
            if (result == TwainResult.Failure)
            {
                throw new TwainException("Failed to set capability.", result, GetStatus());
            }
            if (result != TwainResult.CheckStatus)
            {
                throw new TwainException("Failed to set capability.", result);
            }
        }

        public static short SetCapability(Capabilities capability, short value, Identity applicationId, Identity sourceId)
        {
            return (short)SetBasicCapability(capability, value, TwainType.Int16, applicationId, sourceId);
        }

        public static int SetBasicCapability(Capabilities capability, int rawValue, TwainType twainType, Identity applicationId, Identity sourceId)
        {
            var c = new Capability(capability, twainType, applicationId, sourceId);
            var basicValue = c.GetBasicValue();

            // Check that the device supports the capability
            if (basicValue.ConditionCode != ConditionCode.Success)
            {
                throw new TwainException(string.Format("Unsupported capability {0}", capability), basicValue.ErrorCode, basicValue.ConditionCode);
            }
            if (basicValue.RawBasicValue == rawValue)
            {
                // Value is already set
                return rawValue;
            }

            // TODO: Check the set of Available Values that are supported by the Source for that
            // capability.

            //if (value in set of available values)
            //{
            c.SetValue(rawValue);
            //}

            // Verify that the new values have been accepted by the Source.
            basicValue = c.GetBasicValue();

            // Check that the device supports the capability
            if (basicValue.ConditionCode != ConditionCode.Success)
            {
                throw new TwainException(string.Format("Unexpected failure verifying capability {0}", capability), basicValue.ErrorCode, basicValue.ConditionCode);
            }

            return basicValue.RawBasicValue;
        }

        public static void SetCapability(Capabilities capability, bool value, Identity applicationId, Identity sourceId)
        {
            var c = new Capability(capability, TwainType.Bool, applicationId, sourceId);
            var capResult = c.GetBasicValue();

            // Check that the device supports the capability
            if (capResult.ConditionCode != ConditionCode.Success)
            {
                throw new TwainException(string.Format("Unsupported capability {0}", capability),
                    capResult.ErrorCode, capResult.ConditionCode);
            }

            if (capResult.BoolValue == value)
            {
                // Value is already set
                return;
            }

            c.SetValue(value);

            // Verify that the new values have been accepted by the Source.
            capResult = c.GetBasicValue();

            // Check that the device supports the capability
            if (capResult.ConditionCode != ConditionCode.Success)
            {
                throw new TwainException(string.Format("Unexpected failure verifying capability {0}", capability),
                    capResult.ErrorCode, capResult.ConditionCode);
            }
            else if (capResult.BoolValue != value)
            {
                throw new TwainException(string.Format("Failed to set value for capability {0}", capability),
                    capResult.ErrorCode, capResult.ConditionCode);
            }
        }

        public static bool GetBoolCapability(Capabilities capability, Identity applicationId,
            Identity sourceId)
        {
            var c = new Capability(capability, TwainType.Int16, applicationId, sourceId);
            var capResult = c.GetBasicValue();

            // Check that the device supports the capability
            if (capResult.ConditionCode != ConditionCode.Success)
            {
                throw new TwainException(string.Format("Unsupported capability {0}", capability),
                    capResult.ErrorCode, capResult.ConditionCode);
            }

            return capResult.BoolValue;
        }
    }
}