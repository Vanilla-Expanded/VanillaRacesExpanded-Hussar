
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VREHussars
{

    public static class StaticCollectionsClass
    {

        //This static class stores lists of pawns for different things.


        // A list of pawns and the weapon they are proficient with
        public static IDictionary<Pawn, ThingDef> weaponproficiency_gene_pawns = new Dictionary<Pawn, ThingDef>();
       


        public static void AddWeaponProficiencyGenePawnToList(Pawn thing, ThingDef thingDef)
        {

            if (!weaponproficiency_gene_pawns.ContainsKey(thing))
            {
                weaponproficiency_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveWeaponProficiencyPawnFromList(Pawn thing)
        {
            if (weaponproficiency_gene_pawns.ContainsKey(thing))
            {
                weaponproficiency_gene_pawns.Remove(thing);
            }

        }

       
    }
}
