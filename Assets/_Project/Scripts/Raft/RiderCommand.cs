using UnityEngine;

namespace FartRafters
{
    public struct RiderCommand
    {
        public Vector3 aim;
        public bool gas;

        public RiderCommand(Vector3 aim, bool gas)
        {
            this.aim = aim;
            this.gas = gas;
        }

        public bool HasAim => aim.sqrMagnitude > 0.0001f;
    }

    public abstract class RiderBrain : MonoBehaviour
    {
        public abstract RiderCommand GetCommand(Rider rider);
    }
}
