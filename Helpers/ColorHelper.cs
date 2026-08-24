using System;
using Microsoft.Xna.Framework;

namespace AttackSpeedMeter.Helpers
{
    public static class ColorHelper
    {
        private static readonly Color _low = new(0x4c, 0xaf, 0x50);
        private static readonly Color _high = new(0xFF, 0xAB, 0xAB);

        public static Color Low => _low;
        public static Color High => _high;

        private static Color GetLinearColor(float ratio)
        {
            // Clamp ratio to [0, 1]
            ratio = Math.Max(0, Math.Min(1, ratio));
            // Interpolate each RGB channel between Low and High
            int r = (int)(Low.R + ratio * (High.R - Low.R));
            int g = (int)(Low.G + ratio * (High.G - Low.G));
            int b = (int)(Low.B + ratio * (High.B - Low.B));
            return new Color(r, g, b);
        }

        private static float GetRatio(float low, float high, float raw) =>
            (raw - low) / (high - low);

        /// <summary>
        /// Maps <paramref name="raw"/> between <paramref name="low"/> and <paramref name="high"/>
        /// to a color linearly interpolated between <see cref="Low"/> and <see cref="High"/>.
        /// When <paramref name="high"/> is null or not strictly above <paramref name="low"/>
        /// (no meaningful "next threshold", e.g. equal thresholds after rounding), a fixed
        /// 10% window above <paramref name="low"/> is used instead, mirroring the null case
        /// and avoiding a division by zero.
        /// </summary>
        public static Color GetColor(float low, float? high, float raw)
        {
            if (high == null || high.Value <= low)
            {
                return GetLinearColor(GetRatio(low, low * 1.1f, raw));
            }
            return GetLinearColor(GetRatio(low, high.Value, raw));
        }
    }
}
