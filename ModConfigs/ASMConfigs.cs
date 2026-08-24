using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AttackSpeedMeter.ModConfigs
{
    public class ASMConfigs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Range(0f, 1f)]
        [DefaultValue(0.6f)]
        public float HPosition;

        [Range(0f, 1f)]
        [DefaultValue(0.03f)]
        public float VPosition;

        [Range(300, 600)]
        [DefaultValue(400)]
        public int Width;

        [DefaultValue(true)]
        public bool UseColor;

        [DefaultValue(false)]
        public bool Draggable;
    }
}
