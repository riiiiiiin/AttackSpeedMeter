#nullable enable
using AttackSpeedMeter.ModSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AttackSpeedMeter.Helpers
{
    public static class LocalizationHelper
    {
        public static List<string> GetLegends()
        {
            List<string> legends = [];
            for (int i = 0; i < 7; i++)
            {
                legends.Add(Language.GetTextValue("Mods.AttackSpeedMeter.UITips.Legend." + i));
            }
            return legends;
        }

        public static string GetHeader(DamageClass damageClass, float buff)
        {
            // "Melee Speed"
            string label;
            if (DamageClasses.TryGetVanillaLabelKey(damageClass, out string labelKey))
            {
                // vanilla classes keep their fixed HeaderLabels.<key> entries (original format)
                label = Language.GetTextValue("Mods.AttackSpeedMeter.UITips.HeaderLabels." + labelKey);
            }
            else
            {
                // modded classes match their internal FullName against the variable-length
                // ModdedHeaderLabels list; a class the current language doesn't list falls
                // back to construction
                label = GetModdedHeaderLabel(damageClass.FullName) ?? GetFallbackLabel(damageClass);
            }
            // "Melee Speed: +12%"
            string headerText = Language.GetTextValue("Mods.AttackSpeedMeter.UITips.HeaderFormat");
            headerText = headerText.Replace("[LABEL]", label);
            headerText = headerText.Replace("[BUFF]", FormatHelper.PercentageFloorSigned(buff * 10000f));
            // "- [c/ff90a0:Melee Speed: +12%] -"
            return "- [c/" + FormatHelper.ColorToString(DamageClasses.GetClassColor(damageClass)) + ":" + headerText + "] -";
        }

        // Trailing class-name artifacts stripped from fallback names, in order:
        // compact internal names ("RogueDamageClass") and pretty-printed defaults
        // ("Rogue Damage Class").
        private static readonly string[] _classNameArtifacts = { "DamageClass", " Damage Class" };

        // construct headers from the damage class's display name
        private static String GetFallbackLabel(DamageClass damageClass)
        {
            // texts like "true melee damage" from the mod.
            // DisplayName is normally never null (GetOrRegister materializes it on first
            // access), but a mod may override the virtual property to return null, so a
            // null/empty/key-echoing value all fall back to the pretty-printed internal
            // class name ("TrueMeleeDamageClass" -> "True Melee Damage Class").
            var displayName = damageClass.DisplayName;
            string name = string.IsNullOrEmpty(displayName?.Value) || displayName.Value == displayName.Key
                ? damageClass.PrettyPrintName()
                : displayName.Value;
            name = name.Trim();
            // "Damage" or "damage" to be removed from "true melee damage"
            string suffixesToRemove = Language.GetTextValue("Mods.AttackSpeedMeter.UITips.SuffixesToRemove");
            foreach (string rawSuffix in suffixesToRemove.Split(','))
            {
                string suffix = rawSuffix.Trim();
                if (suffix.Length > 0 && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    // "true melee damage" -> "true melee"
                    name = name.Substring(0, name.Length - suffix.Length).Trim();
                    break;
                }
            }
            // Class-name artifacts: compact internal names end with "DamageClass"
            // ("TrueMeleeDamageClass"), pretty-printed defaults end with " Damage Class"
            // ("True Melee Damage Class"). Stripped before the "damage" strip so
            // "True Melee Damage Class" collapses all the way down to "True Melee".
            foreach (string artifact in _classNameArtifacts)
            {
                if (name.EndsWith(artifact, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - artifact.Length).Trim();
                    break;
                }
            }
            // Many mods ship their damage class display names untranslated even in
            // non-English locales, so always strip the English "damage" suffix once as
            // well (case-insensitive), regardless of the current language, to keep that
            // word out of the header.
            if (name.EndsWith("damage", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - "damage".Length).Trim();
            }
            // in case the damage class is named "damage"... fall back to its internal name
            if (string.IsNullOrEmpty(name))
            {
                name = damageClass.Name ?? "";
            }
            // "true melee" → "True Melee"
            name = NormalizeCase(name);

            // "Speed" to be appended after "True Melee".
            string suffixesToAdd = Language.GetTextValue("Mods.AttackSpeedMeter.UITips.SuffixesToAdd");
            if (!string.IsNullOrEmpty(suffixesToAdd) && !name.EndsWith(suffixesToAdd.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                // "True Melee" -> "True Melee Speed"
                name += suffixesToAdd;
            }
            return name;
        }

        /// <summary>
        /// Title-cases names written entirely without casing (all-lowercase like
        /// "true melee damage", or all-uppercase like "TRUE MELEE DAMAGE"), so the
        /// fallback header reads "True Melee Speed" instead of "true melee Speed".
        /// Names that are already mixed-case (properly capitalized German/French,
        /// brand names, ...) or that have no letter case at all (CJK) are returned
        /// unchanged, which keeps this safe for every locale.
        /// </summary>
        private static string NormalizeCase(string name)
        {
            bool hasLower = name.Any(char.IsLower);
            bool hasUpper = name.Any(char.IsUpper);
            if (hasLower == hasUpper)
            {
                return name;
            }
            // Title-case word by word; this also keeps "rogue's" → "Rogue's" intact,
            // unlike CultureInfo.TextInfo.ToTitleCase which would yield "Rogue'S".
            string[] words = name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
            }
            return string.Join(" ", words);
        }
        /// <summary>
        /// Looks up a modded class's explicit header label in the variable-length
        /// "ModdedHeaderLabels" supplement list, matched by the class's internal FullName
        /// (e.g. "CalamityMod/RogueDamageClass"). Each non-empty line is "FullName: Label";
        /// returns null when the current language has no entry for this class, so the
        /// caller falls back to the programmatic construction.
        /// </summary>
        private static string? GetModdedHeaderLabel(string fullName)
        {
            // "CalamityMod/RogueDamageClass: Rogue Speed" lines, one per class
            string raw = Language.GetTextValue("Mods.AttackSpeedMeter.UITips.ModdedHeaderLabels");
            foreach (string line in raw.Split('\n', '\r'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0)
                    continue;
                int colon = trimmed.IndexOf(':');
                if (colon <= 0)
                    continue;
                if (trimmed.Substring(0, colon).Trim() == fullName)
                    return trimmed.Substring(colon + 1).Trim();
            }
            return null;
        }

        public static string GetSimpleStatus(int useTime)
        {
            return Language.GetTextValue("Mods.AttackSpeedMeter.UITips.UseTime.SimpleStatusTemplate")
                            .Replace("[TIME]", useTime.ToString());
        }

        public static string GetStatus(bool isUseAnimation, int time, float prev, float? next,
                            string? prevColor = null, string? currentColor = null, string? nextColor = null)
        {
            prevColor ??= "ffffff";
            currentColor ??= "ffffff";
            nextColor ??= "ffffff";
            string loc = isUseAnimation ? "UseAnimation" : "UseTime";
            return Language.GetTextValue("Mods.AttackSpeedMeter.UITips." + loc + ".StatusTemplate")
                            .Replace("[TIME]", time.ToString())
                            .Replace("[PREV]", FormatHelper.PercentageFloorSigned(prev))
                            .Replace("[NEXT]", next == null ? "∞" :
                            FormatHelper.PercentageFloorSigned(next.Value))
                            .Replace("[COLORPREV]", prevColor)
                            .Replace("[COLORCURRENT]", currentColor)
                            .Replace("[COLORNEXT]", nextColor);
        }

        public static string GetMultiplier(bool isUseAnimation, float playerMult, float itemMult)
        {
            string loc = isUseAnimation ? "UseAnimation" : "UseTime";
            return Language.GetTextValue("Mods.AttackSpeedMeter.UITips." + loc + ".ExtraMultiplierTemplate")
                            .Replace("[PLAYER]", FormatHelper.PercentageFloor(playerMult))
                            .Replace("[ITEM]", FormatHelper.PercentageFloor(itemMult));
        }

        public static string GetEnterWorldText() =>
            Language.GetTextValue("Mods.AttackSpeedMeter.OtherTips.OnEnterWorld");
    }
}
