using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AttackSpeedMeter.ModSystems
{
    /// <summary>
    /// Damage class cosmetics: header label keys and header colors, matched by the class's
    /// internal FullName (e.g. "Terraria/MeleeDamageClass", "CalamityMod/RogueDamageClass").
    /// This layer is purely cosmetic - every number shown by the meter comes straight from
    /// tML's generic APIs (<see cref="Player.GetTotalAttackSpeed(DamageClass)"/>,
    /// <c>CombinedHooks</c>), which work for ANY registered damage class, including ones
    /// this mod has never heard of. Known classes (vanilla + big mods) get a nicer label
    /// and a class color here; everything else falls back to the class's own DisplayName
    /// and a deterministic pseudo-random color derived from the class's FullName
    /// (<see cref="GetFallbackColor"/>), so unlisted mod classes still get a distinct
    /// header color instead of a generic white.
    /// Vanilla classes (the first eight) keep their fixed <c>HeaderLabels.&lt;key&gt;</c>
    /// entries in the localization file, while modded classes are matched by their
    /// internal FullName directly against the variable-length <c>ModdedHeaderLabels</c>
    /// supplement list ("FullName: Label" lines, newline-separated) that only exists
    /// where a language needs to override the programmatic construction. Classes not
    /// listed there fall back to that construction.
    /// Matching by FullName keeps this fully declarative: no mod lookup calls, no held
    /// DamageClass instance references, and nothing to clean up on unload.
    /// </summary>
    public static class DamageClasses
    {
        // FullName -> localization key suffix for the header label (vanilla classes).
        private static readonly Dictionary<string, string> _vanillaLabelKeys = new()
        {
            { "Terraria/MeleeDamageClass", "MeleeSpeed" },
            { "Terraria/RangedDamageClass", "RangedSpeed" },
            { "Terraria/MeleeNoSpeedDamageClass", "MeleeNoSpeedSpeed" },
            { "Terraria/MagicDamageClass", "MagicSpeed" },
            { "Terraria/SummonMeleeSpeedDamageClass", "SummonMeleeSpeedSpeed" },
            { "Terraria/ThrowingDamageClass", "ThrowingSpeed" },
            { "Terraria/SummonDamageClass", "SummonSpeed" },
            { "Terraria/GenericDamageClass", "GenericSpeed" }
        };

        // FullName -> header color, applied in code instead of inside localization files.
        private static readonly Dictionary<string, Color> _classColors = new()
        {
            { "Terraria/MeleeDamageClass", new Color(0xff, 0x90, 0xa0) },
            { "Terraria/RangedDamageClass", new Color(0xb6, 0xff, 0x90) },
            { "Terraria/MeleeNoSpeedDamageClass", new Color(0xbe, 0x81, 0x89) },
            { "Terraria/MagicDamageClass", new Color(0x90, 0xef, 0xff) },
            { "Terraria/SummonMeleeSpeedDamageClass", new Color(0xff, 0x90, 0xbc) },
            { "Terraria/ThrowingDamageClass", new Color(0xff, 0xaa, 0x90) },
            { "Terraria/SummonDamageClass", new Color(0xfb, 0x90, 0xeb) },
            { "Terraria/GenericDamageClass", Color.White },

            { "CalamityMod/RogueDamageClass", new Color(0xd4, 0x90, 0xff) },
            { "CalamityMod/TrueMeleeDamageClass", new Color(0xff, 0x90, 0xa0) },
            { "CalamityMod/TrueMeleeNoSpeedDamageClass", new Color(0xbe, 0x81, 0x89) },
            { "CalamityMod/MeleeRangedHybridDamageClass", new Color(0xa7, 0x90, 0xff) },
            { "CalamityMod/AverageDamageClass", Color.White },
            { "CalamityMod/AllClassDamageClass", new Color(0x78, 0x70, 0x5a) },

            { "ThoriumMod/HealerDamage", new Color(0xff, 0xd8, 0x90) },
            { "ThoriumMod/HealerTool", new Color(0xff, 0xd8, 0x90) },
            { "ThoriumMod/BardDamage", new Color(0x90, 0xff, 0xd1) }
        };

        // Lowest WCAG contrast ratio against black among the explicitly specified
        // colors above; the fallback below must beat it. Currently that is the
        // "CalamityMod/AllClassDamageClass" color (#78705A, ~4.26:1).
        private static readonly float _minSpecifiedContrast = ComputeMinSpecifiedContrast();

        private static float ComputeMinSpecifiedContrast()
        {
            float min = float.MaxValue;
            foreach (Color color in _classColors.Values)
            {
                min = Math.Min(min, ContrastAgainstBlack(color));
            }
            return min;
        }

        // WCAG 2.x relative luminance (0 = black, 1 = white).
        private static float RelativeLuminance(Color color)
        {
            float R = Linearize(color.R / 255f);
            float G = Linearize(color.G / 255f);
            float B = Linearize(color.B / 255f);
            return 0.2126f * R + 0.7152f * G + 0.0722f * B;
        }

        private static float Linearize(float channel)
            => channel <= 0.03928f ? channel / 12.92f : MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);

        // Contrast ratio of a color against pure black (black's luminance is 0).
        private static float ContrastAgainstBlack(Color color)
            => (RelativeLuminance(color) + 0.05f) / 0.05f;

        // FNV-1a 32-bit: a stable string hash. string.GetHashCode() is randomized per
        // process and must not seed the color.
        private static uint Fnv1a(string text)
        {
            uint hash = 2166136261;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return hash;
        }

        // xorshift32: deterministic PRNG steps fed by the FNV-1a seed.
        private static uint NextRandom(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        // Standard HSL -> RGB, h in [0, 360), s/l in [0, 1].
        private static Color ColorFromHsl(float h, float s, float l)
        {
            float c = (1f - MathF.Abs(2f * l - 1f)) * s;
            float x = c * (1f - MathF.Abs((h / 60f) % 2f - 1f));
            float m = l - c / 2f;
            float r, g, b;
            if (h < 60f) { r = c; g = x; b = 0f; }
            else if (h < 120f) { r = x; g = c; b = 0f; }
            else if (h < 180f) { r = 0f; g = c; b = x; }
            else if (h < 240f) { r = 0f; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0f; b = c; }
            else { r = c; g = 0f; b = x; }
            return new Color((int)MathF.Round((r + m) * 255f), (int)MathF.Round((g + m) * 255f), (int)MathF.Round((b + m) * 255f));
        }

        /// <summary>
        /// Deterministic "random" color for a class with no explicit entry, derived from
        /// the class's internal FullName only ("ModName/ClassName"), so the same class
        /// always gets the same color for every player, session and language. The color
        /// is guaranteed to have a WCAG contrast ratio against black of at least 4.5:1
        /// (WCAG AA for normal text) and strictly above the lowest contrast among the
        /// explicitly specified colors above.
        /// </summary>
        private static Color GetFallbackColor(string fullName)
        {
            uint state = Fnv1a(fullName);
            float hue = (NextRandom(ref state) % 3600) / 10f;                 // 0..360
            float saturation = 0.65f + (NextRandom(ref state) % 350) / 1000f; // 0.65..0.999
            float lightness = 0.5f + (NextRandom(ref state) % 350) / 1000f;   // 0.5..0.849
            float target = Math.Max(4.5f, _minSpecifiedContrast + 0.05f);
            Color color = ColorFromHsl(hue, saturation, lightness);
            // Raising lightness raises relative luminance monotonically, so this always
            // terminates (white reaches 21:1).
            while (ContrastAgainstBlack(color) < target)
            {
                lightness = Math.Min(1f, lightness + 0.05f);
                color = ColorFromHsl(hue, saturation, lightness);
            }
            return color;
        }

        /// <summary>
        /// Tries to get the localization key suffix for a vanilla class's header label.
        /// Vanilla classes keep their fixed HeaderLabels.<key> entries (original format).
        /// </summary>
        public static bool TryGetVanillaLabelKey(DamageClass damageClass, out string labelKey)
            => _vanillaLabelKeys.TryGetValue(damageClass.FullName, out labelKey);

        /// <summary>
        /// Header color for a class, matched by its internal FullName; a deterministic
        /// pseudo-random color derived from the FullName when the class has no explicit
        /// entry (see <see cref="GetFallbackColor"/>).
        /// </summary>
        public static Color GetClassColor(DamageClass damageClass)
        {
            if (_classColors.TryGetValue(damageClass.FullName, out var color))
                return color;
            return GetFallbackColor(damageClass.FullName);
        }

        /// <summary>
        /// Whether this class has a dedicated header label (vanilla or tracked mod class).
        /// </summary>
        public static bool Contains(DamageClass damageClass)
            => _classColors.ContainsKey(damageClass.FullName);
    }
}
