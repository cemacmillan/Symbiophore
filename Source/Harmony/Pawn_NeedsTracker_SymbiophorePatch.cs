using HarmonyLib;
using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    [HarmonyPriority(Priority.Last)]
    public static class Pawn_NeedsTracker_SymbiophorePatch
    {
        [HarmonyPostfix]
        public static void Listener(NeedDef nd, ref bool __result, Pawn ___pawn)
        {
            // If the result is already false or the pawn is null, do nothing
            if (!__result || ___pawn == null)
                return;

            // Check if the need is one of the symbiophore-specific needs
            bool isSymbiophoreNeed = nd.defName == "SymbiophoreMood" || nd.defName == "SymbiophoreFullness";

         
            if (isSymbiophoreNeed)
            {
                // If the pawn does not have the symbiophore hediff, set the result to false
                if (!___pawn.health.hediffSet.HasHediff(DIL_Symbiophore.DefOfs.SymbiophorePsychicEmanation))
                {
                    __result = false;
                }
            }
        }
    }
}