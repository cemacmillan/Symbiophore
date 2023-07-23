using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophoreMoodTracker : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (parent is Pawn pawn && pawn.def.defName == "Symbiophore" && !pawn.Dead)
            {
               

                // Apply the hediff to the symbiophore pawn
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicHarmonization");
                if (hediffDef != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    pawn.health.AddHediff(hediff);
                   
                }
                else
                {
                    Log.Error("Failed to find HediffDef SymbiophorePsychicHarmonization.");
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
          
        }
    }
}
