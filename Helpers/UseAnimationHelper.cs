using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AttackSpeedMeter.Helpers
{
    public static class UseAnimationHelper
    {
        public static int TotalAnimationTimeSim(float attackSpeed, float vanillaSpeedMult,
            float playerSpeedMult, float itemSpeedMult,
            float playerAnimationMult, float itemAnimationMult,
            int useTime, int useAnimation)
        {
            attackSpeed = 1 + ((attackSpeed - 1) * vanillaSpeedMult);
            float totalUseSpeedMult = playerSpeedMult * itemSpeedMult * attackSpeed;
            float totalUseAnimationMult = playerAnimationMult * itemAnimationMult;
            int multipliedUseTime = Math.Max(1, (int)(useTime * (1 / totalUseSpeedMult)));
            int relativeUseAnimation = Math.Max(1, (int)(multipliedUseTime * useAnimation / useTime));
            totalUseAnimationMult *= relativeUseAnimation / (float)useAnimation;
            return Math.Max(1, (int)(useAnimation * totalUseAnimationMult));
        }

        public static int BinarySearchThreshold(Player player, Item item, int targetAnimation)
        {
            var vanillaSpeedMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
            var playerSpeedMult = PlayerLoader.UseSpeedMultiplier(player, item);
            var itemSpeedMult = ItemLoader.UseSpeedMultiplier(item, player);
            var playerAnimationMult = PlayerLoader.UseAnimationMultiplier(player, item);
            var itemAnimationMult = ItemLoader.UseAnimationMultiplier(item, player);
            var useTime = item.useTime;
            var useAnimation = item.useAnimation;

            return ThresholdHelper.BinarySearchThreshold(player, item, targetAnimation,
                attackSpeed => TotalAnimationTimeSim(attackSpeed, vanillaSpeedMult, playerSpeedMult, itemSpeedMult, playerAnimationMult, itemAnimationMult, useTime, useAnimation));
        }
    }
}
