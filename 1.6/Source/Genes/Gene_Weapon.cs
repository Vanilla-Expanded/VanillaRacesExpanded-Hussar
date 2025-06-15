
using RimWorld;
using Verse;


namespace VREHussars
{

    public class Gene_Weapon : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            AddThings();
 
        }

        public override void PostRemove()
        {
            base.PostRemove();
            RemoveThings();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            AddThings();
        }

        

        public void AddThings()
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is false && Active && pawn != null)
            {
                WeaponGeneExtension extension = def.GetModExtension<WeaponGeneExtension>();
                if (extension?.weapon != null)
                {
                    StaticCollectionsClass.AddWeaponProficiencyGenePawnToList(pawn, extension.weapon);
                }

            }
        }

        public void RemoveThings()
        {
            if (PawnGenerator.IsBeingGenerated(pawn) is false && Active)
            {

                WeaponGeneExtension extension = def.GetModExtension<WeaponGeneExtension>();

                if (extension?.weapon != null)
                {
                    StaticCollectionsClass.RemoveWeaponProficiencyPawnFromList(pawn);

                }

            }
        }
    }
}
