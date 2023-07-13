
using RimWorld;
using Verse;

namespace VREHussars
{
    public class Hediff_ChemicalDependency_Luci : Hediff_ChemicalDependency
    {


        public override string TipStringExtra
        {
            get
            {
                string text = "";
                Gene_ChemicalDependency linkedGene = LinkedGene;
                if (linkedGene != null)
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n\n";
                    }
                    text += "GeneDefChemicalNeedDurationDesc".Translate(chemical.label, pawn.Named("PAWN"), "PeriodDays".Translate(10f).Named("DEFICIENCYDURATION"), "PeriodDays".Translate(20f).Named("COMADURATION"), "PeriodDays".Translate(30f).Named("DEATHDURATION")).Resolve();
                    text = text + "\n\n" + "LastIngestedDurationAgo".Translate(chemical.Named("CHEMICAL"), (Find.TickManager.TicksGame - linkedGene.lastIngestedTick).ToStringTicksToPeriod().Named("DURATION")).Resolve();
                }
                return text;
            }
        }

    }
}