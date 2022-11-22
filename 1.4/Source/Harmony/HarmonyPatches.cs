using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VREHussars
{
    


    public class VREHussarsMod : Mod
    {
        public VREHussarsMod(ModContentPack content) : base(content)
        {
            new Harmony("VREHussars.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "Drafted", MethodType.Setter)]
    public static class Pawn_DraftController_Drafted_Patch
    {
        private static void Postfix(Pawn_DraftController __instance, ref bool value)
        {
            if (__instance.pawn.genes != null && !value)
            {
                var gene = __instance.pawn.genes.GetFirstGeneOfType<Gene_Dutiful>();
                if (gene != null)
                {
                    gene.TryDoMentalBreak();
                }
            }
        }
    }

    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public static class MentalStateHandler_TryStartMentalState_Patch
    {
        public static bool Prefix(ref bool __result, MentalStateHandler __instance, MentalStateDef stateDef, string reason = null, 
            bool forceWake = false, bool causedByMood = false, Pawn otherPawn = null, bool transitionSilently = false, 
            bool causedByDamage = false, bool causedByPsycast = false)
        {
            if (__instance.pawn.Drafted && __instance.pawn.genes != null)
            {
                var gene = __instance.pawn.genes.GetFirstGeneOfType<Gene_Dutiful>();
                if (gene != null)
                {
                    gene.RegisterMentalState(stateDef, reason, causedByMood, otherPawn, transitionSilently, causedByDamage, causedByPsycast);
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch]
    public static class MentalWorker_Patches
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var targetMethod = AccessTools.DeclaredMethod(typeof(MentalBreakWorker), "TryStart");
            yield return targetMethod;
            foreach (var subclass in typeof(MentalBreakWorker).AllSubclasses())
            {
                var method = AccessTools.DeclaredMethod(subclass, "TryStart");
                if (method != null)
                {
                    yield return method;
                }
            }
        }

        public static bool Prefix(ref bool __result, MentalBreakWorker __instance, Pawn __0, string __1, bool __2)
        {
            if (__0.Drafted && __0.genes != null)
            {
                var gene = __0.genes.GetFirstGeneOfType<Gene_Dutiful>();
                if (gene != null)
                {
                    gene.RegisterMentalBreak(__instance.def, __1, __2);
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Pawn_PreApplyDamage_Patch
    {
        public static void Postfix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (__instance.genes != null && __instance.genes.HasGene(VREH_DefOf.VREH_BulletproofSkin) &&
                dinfo.Def != DamageDefOf.Blunt && dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp
                && dinfo.ArmorPenetrationInt < 0.2f)
            {
                dinfo.SetBodyRegion(depth: BodyPartDepth.Outside);
                dinfo.SetAllowDamagePropagation(false);
            }
        }
    }

    [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
    public static class ArmorUtility_GetPostArmorDamage_Patch
    {
        public static void Postfix(Pawn pawn, ref DamageDef damageDef, float armorPenetration)
        {
            if (pawn.genes != null && pawn.genes.HasGene(VREH_DefOf.VREH_BulletproofSkin) 
                && damageDef != DamageDefOf.Blunt && damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp 
                && armorPenetration < 0.2f)
            {
                damageDef = DamageDefOf.Blunt;
            }
        }
    }

    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public class GetMaxHealth_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
        {
            if (pawn.genes != null && pawn.genes.HasGene(VREH_DefOf.VREH_Giant))
            {
                __result *= 1.2f;
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "StatOffsetFromGear")]
    public static class StatWorker_StatOffsetFromGear_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool patched = false;
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (!patched && code.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatWorker_StatOffsetFromGear_Patch),
                        nameof(ChangeValueIfNeeded)));
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    patched = true;
                }
            }
        }

        public static float ChangeValueIfNeeded(float val, Thing gear, StatDef stat)
        {
            if (stat == StatDefOf.MoveSpeed && val < 0 && gear.ParentHolder is Pawn_ApparelTracker apparelTracker 
                && apparelTracker.pawn.genes != null && apparelTracker.pawn.genes.HasGene(VREH_DefOf.VREH_Unconstrained))
            {
                return 0f;
            }
            return val;
        }
    }
}
