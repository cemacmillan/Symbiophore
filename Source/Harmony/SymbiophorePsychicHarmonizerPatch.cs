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
                Log.Message("HediffComp_PsychicHarmonizer_CompPostTick_Patch: __instance is null. Allowing original method execution.");
                return true; // If __instance is null, don't do anything and let the original method execute
            }

            Pawn pawn = __instance.parent.pawn;
            if (pawn != null && pawn.Spawned && pawn.RaceProps.Animal && pawn.def.defName == "Symbiophore" && pawn.needs != null)
            {
                Log.Message($"HediffComp_PsychicHarmonizer_CompPostTick_Patch: Patching CompPostTick for {pawn.Name}");

                // Placeholder for custom logic
                // Modify severityAdjustment if needed
                // Example: severityAdjustment = some_custom_value;

                return false; // Prevent the original method from executing
            }

            Log.Message("HediffComp_PsychicHarmonizer_CompPostTick_Patch: Conditions not met. Allowing original method execution.");
            return true;
        }
    }
}
