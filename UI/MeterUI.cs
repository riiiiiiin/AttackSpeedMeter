#nullable enable
using AttackSpeedMeter.Helpers;
using AttackSpeedMeter.ModConfigs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace AttackSpeedMeter.UI
{
    public class MeterUI : UIState
    {
        private readonly AutoTextPanel _mainPanel = new();

        public override void OnInitialize()
        {
            Append(_mainPanel);
        }

        public override void Update(GameTime gameTime)
        {
            // Rebuild the text every other frame to halve the UI churn.
            if (Main.GameUpdateCount % 2 != 0)
            {
                base.Update(gameTime);
                return;
            }

            Player player = Main.player[Main.myPlayer];
            Item item = player.HeldItem;
            DamageClass damageClass = item.DamageType;
            _mainPanel.RemoveAllText();

            // Only show the meter for real weapons: not ammo, not accessories, must have a use time.
            if (item.damage >= 0 && item.useTime > 0 && !(item.ammo > 0) && !item.accessory)
            {
                int useTime = item.useTime;
                bool needAnimationTime = true;
                bool needUseTime = false;
                float attackSpeed = player.GetTotalAttackSpeed(damageClass);
                int totalUseTime = CombinedHooks.TotalUseTime(useTime, player, item);
                int totalAnimationTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
                float vanillaMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];

                // Use Time thresholds. A null next threshold means total use time cannot get any smaller.
                float prevUseTimeThreshold;
                float? nextUseTimeThreshold = null;
                if (totalUseTime != 1)
                {
                    nextUseTimeThreshold = UseTimeHelper.BinarySearchThreshold(player, item, totalUseTime - 1);
                }
                prevUseTimeThreshold = UseTimeHelper.BinarySearchThreshold(player, item, totalUseTime);

                // Animation thresholds. A null next threshold means total animation time cannot get any smaller.
                float prevAnimationThreshold;
                float? nextAnimationThreshold = null;
                var multipliedUseTime = Math.Max(1, (int)(item.useTime * (1 / CombinedHooks.TotalUseSpeedMultiplier(player, item))));
                if (totalAnimationTime != 1 && multipliedUseTime != 1 && (int)(multipliedUseTime * item.useAnimation / item.useTime) > 1)
                {
                    nextAnimationThreshold = UseAnimationHelper.BinarySearchThreshold(player, item, totalAnimationTime - 1);
                }
                prevAnimationThreshold = UseAnimationHelper.BinarySearchThreshold(player, item, totalAnimationTime);

                // Decide which information to display.
                // attackSpeedOnlyAffectsWeaponAnimation means attack speed does not affect use time,
                // so the use-time thresholds would be meaningless.
                // When totalAnimationTime == totalUseTime (or their thresholds coincide),
                // showing both is redundant.
                if (!item.attackSpeedOnlyAffectsWeaponAnimation)
                {
                    needUseTime = true;
                    if (totalAnimationTime == totalUseTime || (prevUseTimeThreshold == prevAnimationThreshold && nextUseTimeThreshold == nextAnimationThreshold))
                    {
                        needAnimationTime = false;
                    }
                }

                _mainPanel.AddText(LocalizationHelper.GetHeader(damageClass, attackSpeed));

                if (needUseTime)
                {
                    AddStatusLines(
                        isAnimation: false,
                        time: totalUseTime,
                        prevThreshold: prevUseTimeThreshold,
                        nextThreshold: nextUseTimeThreshold,
                        attackSpeed: attackSpeed,
                        itemMult: ItemLoader.UseTimeMultiplier(item, player) * (1 / ItemLoader.UseSpeedMultiplier(item, player)),
                        playerMult: PlayerLoader.UseTimeMultiplier(player, item) * (1 / PlayerLoader.UseSpeedMultiplier(player, item)),
                        vanillaMult: vanillaMult);
                }
                else
                {
                    _mainPanel.AddText(LocalizationHelper.GetSimpleStatus(totalUseTime));
                }

                if (needAnimationTime)
                {
                    AddStatusLines(
                        isAnimation: true,
                        time: totalAnimationTime,
                        prevThreshold: prevAnimationThreshold,
                        nextThreshold: nextAnimationThreshold,
                        attackSpeed: attackSpeed,
                        itemMult: ItemLoader.UseAnimationMultiplier(item, player) * (1 / ItemLoader.UseSpeedMultiplier(item, player)),
                        playerMult: PlayerLoader.UseAnimationMultiplier(player, item) * (1 / PlayerLoader.UseSpeedMultiplier(player, item)),
                        vanillaMult: vanillaMult);
                }
            }
            else
            {
                // No weapon held: show the legend instead.
                foreach (string legend in LocalizationHelper.GetLegends())
                {
                    _mainPanel.AddText(legend);
                }
            }

            _mainPanel.UpdateText();
            base.Update(gameTime);
        }

        /// <summary>
        /// Adds the status line (and the extra-multiplier line, when anything is actually
        /// boosting the rate) for one of the two time values, use time or use animation.
        /// </summary>
        private void AddStatusLines(bool isAnimation, int time, float prevThreshold, float? nextThreshold,
            float attackSpeed, float itemMult, float playerMult, float vanillaMult)
        {
            string? prevColor = null, currentColor = null, nextColor = null;
            if (ModContent.GetInstance<ASMConfigs>().UseColor)
            {
                prevColor = FormatHelper.ColorToString(ColorHelper.Low);
                currentColor = FormatHelper.ColorToString(ColorHelper.GetColor(prevThreshold, nextThreshold, attackSpeed * 10000));
                nextColor = FormatHelper.ColorToString(ColorHelper.High);
            }

            _mainPanel.AddText(LocalizationHelper.GetStatus(isAnimation, time, prevThreshold, nextThreshold, prevColor, currentColor, nextColor));

            // Show the extra multipliers only when something actually changes the rate.
            // The "item" side shows the modded item multiplier when present, otherwise the
            // vanilla bonus multiplier, otherwise a plain 1.00.
            float displayedItemMult;
            if (Math.Abs(itemMult - 1) >= 1e-4f)
            {
                displayedItemMult = 1 / itemMult;
            }
            else if (Math.Abs(vanillaMult - 1) >= 1e-4f)
            {
                displayedItemMult = vanillaMult;
            }
            else
            {
                displayedItemMult = 1f;
            }
            if (Math.Abs(playerMult - 1) >= 1e-4f || Math.Abs(displayedItemMult - 1) >= 1e-4f)
            {
                _mainPanel.AddText(LocalizationHelper.GetMultiplier(isAnimation, 1 / playerMult, displayedItemMult));
            }
        }
    }
}
