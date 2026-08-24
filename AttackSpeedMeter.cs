using AttackSpeedMeter.ModSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace AttackSpeedMeter
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class AttackSpeedMeter : Mod
	{
		public static ModKeybind MeterKey;
		public override void Load()
		{
			MeterKey = KeybindLoader.RegisterKeybind(this, "Toggle Attack Speed Meter", "K");

		}

		public override void Unload()
		{
			MeterKey = null;
		}
	}
}
