using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VREHussars
{

    // Aptitude genes are no longer generated for unique weapons. A save that carries one is
    // pointed at the base weapon's gene, which now covers the unique variant as well.
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public static class VREHussars_BackCompatibility_BackCompatibleDefName_Patch
    {
        public static void Postfix(Type defType, ref string __result)
        {
            if (defType != typeof(GeneDef) || __result.NullOrEmpty() || DefDatabase<GeneDef>.GetNamedSilentFail(__result) != null)
            {
                return;
            }
            foreach (WeaponGeneTemplateDef template in DefDatabase<WeaponGeneTemplateDef>.AllDefs)
            {
                string prefix = template.defName + "_";
                if (!__result.StartsWith(prefix))
                {
                    continue;
                }
                ThingDef weapon = DefDatabase<ThingDef>.GetNamedSilentFail(__result.Substring(prefix.Length));
                if (weapon == null)
                {
                    continue;
                }
                string baseGene = prefix + UniqueWeaponUtility.GetBaseWeapon(weapon).defName;
                if (baseGene != __result && DefDatabase<GeneDef>.GetNamedSilentFail(baseGene) != null)
                {
                    __result = baseGene;
                    return;
                }
            }
        }
    }
}
