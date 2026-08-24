using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AttackSpeedMeter.Helpers
{
    public static class UseTimeHelper
    {
        public static int TotalUseTimeSim(float attackSpeed, float vanillaSpeedMult,
            float playerSpeedMult, float itemSpeedMult,
            float playerTimeMult, float itemTimeMult,
            int useTime)
        {
            attackSpeed = 1 + ((attackSpeed - 1) * vanillaSpeedMult);
            float totalUseSpeedMult = playerSpeedMult * itemSpeedMult * attackSpeed;
            float totalUseTimeMult = playerTimeMult * itemTimeMult;
            totalUseTimeMult /= totalUseSpeedMult;
            return Math.Max(1, (int)(useTime * totalUseTimeMult));
        }

        public static int BinarySearchThreshold(Player player, Item item, int targetUseTime)
        {
            var vanillaSpeedMult = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
            var playerSpeedMult = PlayerLoader.UseSpeedMultiplier(player, item);
            var itemSpeedMult = ItemLoader.UseSpeedMultiplier(item, player);
            var playerTimeMult = PlayerLoader.UseTimeMultiplier(player, item);
            var itemTimeMult = ItemLoader.UseTimeMultiplier(item, player);
            var useTime = item.useTime;

            return ThresholdHelper.BinarySearchThreshold(player, item, targetUseTime,
                attackSpeed => TotalUseTimeSim(attackSpeed, vanillaSpeedMult, playerSpeedMult, itemSpeedMult, playerTimeMult, itemTimeMult, useTime));
        }
    }
}
