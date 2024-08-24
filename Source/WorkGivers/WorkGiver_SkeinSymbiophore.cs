using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class WorkGiver_SkeinSymbiophore : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef => DefDatabase<JobDef>.GetNamed("SkeinSymbiophore");

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompCustomSkeinable>();
        }
    }
}
