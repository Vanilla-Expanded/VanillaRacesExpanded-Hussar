
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;




namespace VREHussars
{

    public class Gene_RandomWeapon : Gene
    {
        public List<GeneDef> genes = new List<GeneDef>();

        public override void PostAdd()
        {
            base.PostAdd();


            genes = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.defName.Contains("VREHT_") && x.defName!=this.def.defName).ToList();

            pawn.genes.AddGene(genes.RandomElement(),true);
            pawn.genes.RemoveGene(this);






        }

       

    }
}
