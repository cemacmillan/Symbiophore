using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace DIL_Symbiophore
{
    public class JobDriver_Skein : JobDriver_GatherAnimalBodyResources
    {
        private float gatherProgress; // Add gatherProgress field

        protected override float WorkTotal => 1700f; // Adjust as necessary

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompCustomSkeinable>(); // Use the custom component
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref gatherProgress, "gatherProgress", 0f);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = ToilMaker.MakeToil("MakeNewToils");
            wait.initAction = delegate
            {
                Pawn actor2 = wait.actor;
                Pawn obj = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                actor2.pather.StopDead();
                PawnUtility.ForceWait(obj, 15000, null, maintainPosture: true);
            };
            wait.tickAction = delegate
            {
                Pawn actor = wait.actor;
                actor.skills.Learn(SkillDefOf.Animals, 0.13f);
                gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed);
                if (gatherProgress >= WorkTotal)
                {
                    ((CompCustomSkeinable)GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A))).CustomGathered(pawn);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            wait.AddFinishAction(delegate
            {
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            });
            wait.FailOnDespawnedOrNull(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(() => GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A)).ActiveAndFull ? JobCondition.Ongoing : JobCondition.Incompletable);
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => gatherProgress / WorkTotal);
            wait.activeSkill = () => SkillDefOf.Animals;
            yield return wait;
        }
    }
}