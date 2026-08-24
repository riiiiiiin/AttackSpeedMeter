using System;
using Microsoft.Xna.Framework;

namespace AttackSpeedMeter.Helpers
{
    internal static class FormatHelper
    {
        public static string ColorToString(Color color) =>
            $"{color.R:X2}{color.G:X2}{color.B:X2}";

        public static float SafeFloor(float input)
        {
            if (Math.Ceiling(input) - input < 0.01f)
                return (float)Math.Ceiling(input);
            return (float)Math.Floor(input);
        }

        // percent is in 1/10000 units (attack speed 1.12 is 11200), offset by 10000
        // so a plain attack speed shows as the signed difference ("+12.00%", "-5.00%").
        public static string PercentageFloorSigned(float percent) =>
            (SafeFloor(percent - 10000f) / 100f).ToString("+#0.00;-#0.00;0") + "%";

        // percent is a plain multiplier (1.15 -> "115%").
        public static string PercentageFloor(float percent) =>
            (SafeFloor(percent * 10000f) / 100f).ToString() + "%";
    }
}
