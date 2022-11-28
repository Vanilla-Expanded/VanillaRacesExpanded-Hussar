
using System.Collections.Generic;
using RimWorld;
using Verse;
using VREHussars;

namespace VREHussars
{
    public class Gene_ChemicalDependencyLuci : Gene_ChemicalDependency
    {


        public override void PostAdd()
        {
            if (!ModLister.CheckBiotech("Chemical dependency"))
            {
                return;
            }
           
            if (def.chemical.addictionHediff != null)
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.chemical.addictionHediff);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
            Hediff_ChemicalDependency hediff_ChemicalDependency = (Hediff_ChemicalDependency)HediffMaker.MakeHediff(VREH_DefOf.VREH_GeneticDrugNeed_Luci, pawn);
            hediff_ChemicalDependency.chemical = def.chemical;
            pawn.health.AddHediff(hediff_ChemicalDependency);
            lastIngestedTick = Find.TickManager.TicksGame;
        }

    }
}
