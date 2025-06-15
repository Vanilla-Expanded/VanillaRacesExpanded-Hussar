using HarmonyLib;
using UnityEngine;
using Verse;

namespace VREHussars
{

	public class VREHussarsMod : Mod
	{
		public static HussarsSettings settings;

		public VREHussarsMod(ModContentPack content) : base(content)
		{
			new Harmony("VREHussars.Mod").PatchAll();
			settings = GetSettings<HussarsSettings>();
		}

		public override string SettingsCategory() => "VRE_HussarsSettings".Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			var listing = new Listing_Standard();
			listing.Begin(inRect);
			listing.CheckboxLabeled("VREH_Label_onlyVanillaWeapon".Translate(), ref settings.onlyVanillaWeapon, "VREH_ToolTip_onlyVanillaWeapon".Translate());
			listing.CheckboxLabeled("VREH_Label_allowNonBiocodableWeapon".Translate(), ref settings.allowNonBiocodableWeapon, "VREH_ToolTip_allowNonBiocodableWeapon".Translate());
			listing.End();
		}
	}

	public class HussarsSettings : ModSettings
	{

		public bool onlyVanillaWeapon = false;
		public bool allowNonBiocodableWeapon = true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref onlyVanillaWeapon, "onlyVanillaWeapon", defaultValue: false);
			Scribe_Values.Look(ref allowNonBiocodableWeapon, "allowNonBiocodableWeapon", defaultValue: true);
		}
	}

}
