using AttackSpeedMeter.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace AttackSpeedMeter.ModSystems
{
    public class UISystem : ModSystem
    {
        internal UserInterface MeterInterface;
        internal MeterUI MeterUI;
        private GameTime _lastUpdateUiGameTime;

        internal bool IsMeterClosed() => MeterInterface?.CurrentState == null;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MeterInterface = new UserInterface();
                MeterUI = new MeterUI();
                MeterUI.Activate();
            }
        }

        public override void Unload()
        {
            MeterInterface = null;
            MeterUI = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (MeterInterface?.CurrentState != null)
            {
                MeterInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex == -1)
            {
                // Fall back to appending the layer so the meter stays visible even if
                // the vanilla layer name changes in a future version.
                mouseTextIndex = layers.Count;
            }
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "AttackSpeedMeter: MeterInterface",
                () =>
                {
                    if (_lastUpdateUiGameTime != null && MeterInterface?.CurrentState != null)
                    {
                        MeterInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                    }
                    return true;
                },
                InterfaceScaleType.UI));
        }

        internal void CloseMeter()
            => MeterInterface?.SetState(null);

        internal void OpenMeter()
            => MeterInterface?.SetState(MeterUI);

        public void ToggleMeter()
        {
            if (IsMeterClosed())
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                OpenMeter();
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                CloseMeter();
            }
        }
    }
}
