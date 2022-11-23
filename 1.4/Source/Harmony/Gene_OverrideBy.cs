using HarmonyLib;
using System.Collections.Generic;
using Verse;
using VREHussars;

namespace VREHussars
{

    [HarmonyPatch(typeof(Gene), "OverrideBy")]
    public static class VREHussars_Gene_OverrideBy_Patch
    {
        public static void Postfix(Gene __instance, Gene overriddenBy)
        {
            if (overriddenBy != null)
            {
                RemoveGeneEffects(__instance);
            }
            else ApplyGeneEffects(__instance);
        }
        public static void RemoveGeneEffects(Gene gene)
        {

            WeaponGeneExtension extension = gene.def.GetModExtension<WeaponGeneExtension>();
            if (extension != null)
            {
                if (extension.weapon != null)
                {
                    StaticCollectionsClass.RemoveWeaponProficiencyPawnFromList(gene.pawn);

                }
 
            }
        }
        public static void ApplyGeneEffects(Gene gene)
        {
            WeaponGeneExtension extension = gene.def.GetModExtension<WeaponGeneExtension>();
            if (extension != null)
            {
               
                if (extension.weapon != null)
                {
                    StaticCollectionsClass.AddWeaponProficiencyGenePawnToList(gene.pawn, extension.weapon);
                }
              


            }
        }

    }
}