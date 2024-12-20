﻿using RimWorld;
using Verse;

namespace DIL_Symbiophore
{

    public class WorkGiver_SkeinSymbiophore : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef => DefDatabase<JobDef>.GetNamed("SkeinSymbiophore");

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            var comp = animal.TryGetComp<CompSkeinable>();
     /*       if (comp != null && comp.ActiveAndFull)
            {
                Log.Message($"[Symbiophore] {animal.LabelShort} is ready for skeining with progress: {comp.SkeinProgress}");
            }*/
            return comp;
        }
    }
}
