using System;
using Terraria;

namespace AttackSpeedMeter.Helpers
{
    /// <summary>
    /// Shared binary search used to find the attack speed thresholds for both the
    /// use time and the use animation of a weapon. UseTimeHelper and UseAnimationHelper
    /// only supply their own simulation functions; the search itself lives here.
    /// </summary>
    internal static class ThresholdHelper
    {
        // Search window for the attack speed multiplier.
        private const float Left = -0.9900f;
        private const float Right = 20.0000f;
        private const float Epsilon = 0.0001f;

        /// <summary>
        /// Finds the attack speed at which <paramref name="simulate"/> stops producing
        /// values greater than <paramref name="targetTime"/>, and returns that boundary
        /// scaled to 1/10000 units (the unit the meter displays).
        /// </summary>
        public static int BinarySearchThreshold(Player player, Item item, int targetTime, Func<float, int> simulate)
        {
            float left = Left;
            float right = Math.Max(Right, player.GetTotalAttackSpeed(item.DamageType) * 3);
            while (right - left > Epsilon)
            {
                float mid = (left + right) / 2;
                if (simulate(mid) > targetTime)
                {
                    left = mid;
                }
                else
                {
                    right = mid;
                }
            }

            var rightHigh = Math.Ceiling(right * 10000) / 10000;
            var rightLow = Math.Floor(right * 10000) / 10000;
            if (simulate((float)rightLow) <= targetTime)
            {
                return (int)FormatHelper.SafeFloor((float)(rightLow * 10000));
            }
            return (int)FormatHelper.SafeFloor((float)(rightHigh * 10000));
        }
    }
}
