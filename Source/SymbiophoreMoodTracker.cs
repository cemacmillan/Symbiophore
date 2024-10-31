using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophoreMoodTracker : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TryAddHediff();
        }

        public override void CompTick()
        {
            base.CompTick();
           
        }

        private void TryAddHediff()
        {
            if (parent is Pawn pawn && pawn.def.defName == "Symbiophore" && !pawn.Dead)
            {
                // Check if the Hediff is already applied
                if (!pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation")))
                {
                    // Get Hediff onto the pawn
                    HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation");
                    if (hediffDef != null)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        pawn.health.AddHediff(hediff);
                        Log.Message($"[SymbiophoreMoodTracker] Added {hediffDef.defName} to {pawn.LabelShort}.");
                    }
                    else
                    {
                        Log.Error("Failed to find HediffDef 'SymbiophorePsychicEmanation'.");
                    }
                }
            }
        }
    }
}