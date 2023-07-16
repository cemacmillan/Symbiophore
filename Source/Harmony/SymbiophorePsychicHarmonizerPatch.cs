using HarmonyLib;
using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    [HarmonyPatch(typeof(HediffComp_PsychicHarmonizer))]
    [HarmonyPatch("CompPostTick")]
    public static class HediffComp_PsychicHarmonizer_CompPostTick_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(HediffComp_PsychicHarmonizer __instance, ref float severityAdjustment)
        {
            if (__instance == null)
            {
                return true; // If __instance is null, don't do anything and let the original method execute
            }

            Pawn pawn = __instance.parent.pawn;
            if (pawn != null && pawn.Spawned && pawn.RaceProps.Animal && pawn.def.defName == "Symbiophore" && pawn.needs != null)
            {
                // Modify severityAdjustment if needed

                return false; // Prevent the original method from executing
            }

            return true;
        }
    }
}
