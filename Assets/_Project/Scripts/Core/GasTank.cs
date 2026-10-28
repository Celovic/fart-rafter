using UnityEngine;

namespace FartRafters
{
    public class GasTank
    {
        float sinceFire = float.MaxValue;

        public float Amount { get; private set; }
        public float Capacity { get; private set; }
        public bool Exhausted { get; private set; }
        public float Normalized => Capacity > 0f ? Amount / Capacity : 0f;

        public void Reset(float capacity)
        {
            Capacity = capacity;
            Amount = capacity;
            Exhausted = false;
            sinceFire = float.MaxValue;
        }

        public void SetNormalized(float normalized, float capacity)
        {
            Capacity = capacity;
            Amount = Mathf.Clamp01(normalized) * capacity;
            Exhausted = normalized <= 0.01f;
        }

        public void Fill()
        {
            Amount = Capacity;
            Exhausted = false;
        }

        public bool Tick(float deltaTime, bool wantsGas, float capacity, float refillPerSecond, float refillDelay, float recoverThreshold, bool infinite = false)
        {
            if (!Mathf.Approximately(capacity, Capacity))
            {
                float ratio = Normalized;
                Capacity = capacity;
                Amount = ratio * capacity;
            }

            if (infinite)
            {
                Exhausted = false;
                Amount = Mathf.Min(Capacity, Amount + refillPerSecond * deltaTime);
            }
            bool firing = wantsGas && !Exhausted && Amount > 0f;
            if (firing)
            {
                if (!infinite)
                {
                    Amount = Mathf.Max(0f, Amount - deltaTime);
                }
                sinceFire = 0f;
                if (Amount <= 0f)
                {
                    Exhausted = true;
                }
                return true;
            }

            sinceFire += deltaTime;
            if (sinceFire >= refillDelay)
            {
                Amount = Mathf.Min(Capacity, Amount + refillPerSecond * deltaTime);
            }
            if (Exhausted && Normalized >= recoverThreshold)
            {
                Exhausted = false;
            }
            return false;
        }
    }
}
