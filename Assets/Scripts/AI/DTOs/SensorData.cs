using System;
using AI.GOAP.Enums;
using AI.GOAP.Sensor;
using UnityEngine.Serialization;

namespace AI.GOAP.DTO
{
    [Serializable]
    public struct SensorData
    {
        public SensorName name;
        public SensorBase sensor;
    }
}
