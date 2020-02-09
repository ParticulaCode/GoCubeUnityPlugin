using System;
using UnityEngine;

namespace Particula.Common {

    public interface IBattery {
        event Action onBatteryUpdate;
        float batteryPercent { get; }
    }

    public interface ICube {
        Quaternion rotation { get; }
    }

    public interface IPhysicalCube : ICube, IBattery {
        void ResetCubeOrientation(Quaternion? q = null);
        void DisableIMU();
        void EnableIMU();
    }

    public interface IFace {
        int id { get; }
        float angle { get; }
    }
}
