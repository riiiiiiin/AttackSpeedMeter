using AttackSpeedMeter.Helpers;
using AttackSpeedMeter.ModSystems;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace AttackSpeedMeter
{
    public class ASMPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (AttackSpeedMeter.MeterKey?.JustPressed == true)
            {
                ModContent.GetInstance<UISystem>().ToggleMeter();
            }
        }

        public override void OnEnterWorld()
        {
            // Remind the player only when the toggle keybind is actually unbound.
            if (AttackSpeedMeter.MeterKey?.GetAssignedKeys()?.Count == 0)
            {
                Main.NewText(LocalizationHelper.GetEnterWorldText());
            }
        }
    }
}
