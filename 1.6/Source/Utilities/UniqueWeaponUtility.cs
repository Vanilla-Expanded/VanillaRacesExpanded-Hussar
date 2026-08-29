using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VREHussars
{

    [StaticConstructorOnStartup]
    public static class UniqueWeaponUtility
    {

        // Unique weapons (Odyssey) mapped to the base weapon they are a variant of, taken from their description hyperlinks.
        // Vanilla uniques link a single base weapon; if more are linked, only a "<base>_Unique" defName match is trusted.
        private static readonly Dictionary<ThingDef, ThingDef> baseWeapons = new Dictionary<ThingDef, ThingDef>();

        static UniqueWeaponUtility()
        {
            if (!ModsConfig.OdysseyActive)
            {
                return;
            }
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.IsWeapon && def.HasComp(typeof(CompUniqueWeapon)) && def.descriptionHyperlinks != null)
                {
                    List<ThingDef> linkedWeapons = def.descriptionHyperlinks.Select(x => x.def).OfType<ThingDef>()
                        .Where(x => x.IsWeapon && !x.HasComp(typeof(CompUniqueWeapon))).ToList();
                    ThingDef baseWeapon = linkedWeapons.Count == 1
                        ? linkedWeapons[0]
                        : linkedWeapons.FirstOrDefault(x => x.defName + "_Unique" == def.defName);
                    if (baseWeapon != null)
                    {
                        baseWeapons[def] = baseWeapon;
                    }
                }
            }
        }

        public static ThingDef GetBaseWeapon(ThingDef weapon)
        {
            return baseWeapons.TryGetValue(weapon, out ThingDef result) ? result : weapon;
        }
    }
}
