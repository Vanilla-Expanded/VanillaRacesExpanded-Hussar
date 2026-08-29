
using RimWorld;
using Verse;
namespace VREHussars
{

    public class ConditionalStatAffecter_WeaponProficiency : ConditionalStatAffecter
    {
        public override string Label => "VREH_WeaponAptitude".Translate();

        public override bool Applies(StatRequest req)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            if (req.HasThing && req.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
            {


                if (pawn.equipment != null && pawn.equipment.Primary != null)
                {
                    ThingDef weapon = pawn.equipment.Primary.def;
                    if (StaticCollectionsClass.weaponproficiency_gene_pawns.TryGetValue(pawn, out ThingDef proficiency) &&
                        (proficiency == weapon || proficiency == UniqueWeaponUtility.GetBaseWeapon(weapon)))
                    {
                        return true;
                    }

                }
            }
            return false;
        }
    }
}
