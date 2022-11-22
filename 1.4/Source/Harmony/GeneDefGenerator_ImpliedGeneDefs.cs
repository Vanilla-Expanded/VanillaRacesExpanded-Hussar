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

namespace VREHussars
{




    [HarmonyPatch(typeof(GeneDefGenerator))]
    [HarmonyPatch("ImpliedGeneDefs")]

    public static class VREHussars_GeneDefGenerator_ImpliedGeneDefs_Patch
    {

        public static IEnumerable<GeneDef> Postfix(IEnumerable<GeneDef> values)
        {
            List<GeneDef> resultingList = values.ToList();

            foreach (WeaponGeneTemplateDef template in DefDatabase<WeaponGeneTemplateDef>.AllDefs)
            {
                Log.Message("detected "+template.defName);

                List<ThingDef> listOfWeapons = DefDatabase<ThingDef>.AllDefs.Where(element => (element.weaponTags?.Count > 0)).ToList();

                foreach (ThingDef weapon in listOfWeapons)
                {
                    resultingList.Add(GetFromTemplate(template, weapon, weapon.index * 1000));
                }


            }



            return resultingList;




        }

        public static GeneDef GetFromTemplate(WeaponGeneTemplateDef template, ThingDef def, int displayOrderBase)
        {
            GeneDef geneDef = new GeneDef
            {
                defName = template.defName + "_" + def.defName,
                geneClass = template.geneClass,
                label = template.label.Formatted(def.label),
                iconPath = def.graphicData.texPath,
                //description = ResolveDescription(),
                labelShortAdj = template.labelShortAdj.Formatted(def.label),
                selectionWeight = template.selectionWeight,
                biostatCpx = template.biostatCpx,
                biostatMet = template.biostatMet,
                displayCategory = template.displayCategory,
                displayOrderInCategory = displayOrderBase + template.displayOrderOffset,
                minAgeActive = template.minAgeActive,
                modContentPack = template.modContentPack
            };
            if (!template.exclusionTagPrefix.NullOrEmpty())
            {
                geneDef.exclusionTags = new List<string> { template.exclusionTagPrefix + "_" + def.defName };
            }

            return geneDef;

        }



    }

}
