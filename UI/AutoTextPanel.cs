#nullable enable
using AttackSpeedMeter.ModConfigs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace AttackSpeedMeter.UI
{
    public class AutoTextPanel : UIPanel
    {
        private const float VPadding = 3;
        private const float VLine = 23;
        private const float InitialHeight = 200;
        private int _childIndex;
        private bool _dragging;
        private float _xBias;
        private float _yBias;

        private static float VPos(int index) => VPadding + VLine * index;

        public override void OnInitialize()
        {
            Width.Set(ModContent.GetInstance<ASMConfigs>().Width, 0);
            Height.Set(InitialHeight, 0);
            HAlign = ModContent.GetInstance<ASMConfigs>().HPosition;
            VAlign = ModContent.GetInstance<ASMConfigs>().VPosition;
        }

        public void AddText(string text)
        {
            UIText temp = new(text)
            {
                HAlign = 0.5f
            };
            temp.Top.Set(VPos(_childIndex), 0);
            _childIndex++;
            Append(temp);
        }

        public override void Update(GameTime gameTime)
        {
            if (ModContent.GetInstance<ASMConfigs>().Draggable)
            {
                // Only start a drag when the press actually lands on the panel;
                // otherwise clicking anywhere (attacking, mining) would steal the mouse.
                if (!_dragging && Main.mouseLeft && IsMouseHovering)
                {
                    _dragging = true;
                    _xBias = Main.mouseX - HAlign * Main.screenWidth;
                    _yBias = Main.mouseY - VAlign * Main.screenHeight;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                if (_dragging)
                {
                    // Keep following the mouse while held, even when the cursor leaves
                    // the panel; only the release ends the drag.
                    if (Main.mouseLeftRelease)
                    {
                        _dragging = false;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else
                    {
                        float hAlign = Math.Clamp(((float)Main.mouseX - _xBias) / Main.screenWidth, 0f, 1f);
                        float vAlign = Math.Clamp(((float)Main.mouseY - _yBias) / Main.screenHeight, 0f, 1f);
                        ModContent.GetInstance<ASMConfigs>().HPosition = hAlign;
                        ModContent.GetInstance<ASMConfigs>().VPosition = vAlign;
                    }
                }
            }
            HAlign = ModContent.GetInstance<ASMConfigs>().HPosition;
            VAlign = ModContent.GetInstance<ASMConfigs>().VPosition;
            base.Update(gameTime);
        }

        public void UpdateText()
        {
            Width.Set(ModContent.GetInstance<ASMConfigs>().Width, 0);
            Height.Set((_childIndex + 1) * VLine + VPadding, 0);
        }

        public void RemoveAllText()
        {
            _childIndex = 0;
            RemoveAllChildren();
        }
    }
}
