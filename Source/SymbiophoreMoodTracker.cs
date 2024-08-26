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
               

                // Get Hediff onto the pawn
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation");
                if (hediffDef != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    pawn.health.AddHediff(hediff);
                   
                }
                else
                {
                    Log.Error("Failed to find HediffDef SymbiophorePsychicEmanation. Most likely there has been a massive EMP pulse in your vicinity disrupting your computer. Notify the developer of this mod.");
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
          
        }
    }
}
