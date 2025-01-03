using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using static RimWorld.BaseGen.SymbolStack;

namespace VREHussars
{




    [HarmonyPatch(typeof(GeneDefGenerator))]
    [HarmonyPatch("ImpliedGeneDefs")]

    public static class VREHussars_GeneDefGenerator_ImpliedGeneDefs_Patch
    {

        public static IEnumerable<GeneDef> Postfix(IEnumerable<GeneDef> values)
        {
            List<GeneDef> resultingList = values.ToList();
            List<string> blackListedWeapons = new();
            List<BlackListedWeaponsDef> allBlackListedWeapons = DefDatabase<BlackListedWeaponsDef>.AllDefsListForReading;
            foreach (BlackListedWeaponsDef individualList in allBlackListedWeapons)
            {
                blackListedWeapons.AddRange(individualList.blackListedWeapons);
            }
            List<ThingDef> listOfWeapons = DefDatabase<ThingDef>.AllDefs.Where(WeaponFilter(blackListedWeapons)).ToList();
            foreach (WeaponGeneTemplateDef template in DefDatabase<WeaponGeneTemplateDef>.AllDefs)
            {
                foreach (ThingDef weapon in listOfWeapons)
                {
                    resultingList.Add(GetFromTemplate(template, weapon, weapon.index * 1000));
                }
            }
            return resultingList;
        }

        public static Func<ThingDef, bool> WeaponFilter(List<string> blackListedWeapons)
        {
            return element => element.weaponTags?.Count > 0 
                         && element.destroyOnDrop == false 
                         && element.hasInteractionCell == false 
                         && element.building == null 
                         && !element.HasComp(typeof(CompExplosive)) 
                         && element.HasComp(typeof(CompQuality)) 
                         && element.recipeMaker?.workSkill != null 
                         && !blackListedWeapons.Contains(element.defName)
                         && (!VREHussarsMod.settings.onlyVanillaWeapon || element.modContentPack?.IsOfficialMod == true)
                         && (VREHussarsMod.settings.allowNonBiocodableWeapon || element.HasComp(typeof(CompBiocodable)));
        }

        public static GeneDef GetFromTemplate(WeaponGeneTemplateDef template, ThingDef def, int displayOrderBase)
        {
            GeneDef geneDef = new()
            {
                defName = template.defName + "_" + def.defName,
                geneClass = template.geneClass,
                label = template.label.Formatted(def.label),
                iconPath = def.graphicData.texPath,
                description = template.description.Formatted(def.LabelCap),
                labelShortAdj = template.labelShortAdj.Formatted(def.label),
                selectionWeight = template.selectionWeight,
                biostatCpx = template.biostatCpx,
                biostatMet = template.biostatMet,
                displayCategory = template.displayCategory,
                displayOrderInCategory = displayOrderBase + template.displayOrderOffset,
                minAgeActive = template.minAgeActive,
                modContentPack = template.modContentPack,
                modExtensions = new List<DefModExtension> { 
                    new WeaponGeneExtension { 
                        weapon = def
                    } 
                },
                conditionalStatAffecters = new List<ConditionalStatAffecter> { 
                    new ConditionalStatAffecter_WeaponProficiency {statFactors= new List<StatModifier>
                        {
                            new StatModifier{stat=StatDefOf.ShootingAccuracyPawn,value=1.5f},
                            new StatModifier{stat=StatDefOf.MeleeHitChance,value=1.5f}
                        }
                    } 
                }
            };

           
            if (!template.exclusionTagPrefix.NullOrEmpty())
            {
                geneDef.exclusionTags = new List<string> { template.exclusionTagPrefix};
            }

            return geneDef;

        }

       



    }

}
