using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace DIL_Symbiophore
{
    public class CompSkeinable : CompHasGatherableBodyResource
    {
        protected override int GatherResourcesIntervalDays => Props.gatherResourcesIntervalDays;
        protected override int ResourceAmount => CalculateResourceAmount();
        protected override ThingDef ResourceDef => Props.resourceDef;
        protected override string SaveKey => "skeinProgress";
        protected float lastMoodProxy;

        // Store the skeinProgress (which replaces fullness in your system)
        private float skeinProgress;

        public CompProperties_Skeinable Props => (CompProperties_Skeinable)props;

        // Disable CompTick() since we don't need it
        public override void CompTick()
        {
            // Intentionally do nothing, because we handle progress in CompTickRare().
        }

        /* but the above choice has implications */

        public void NotifyGamePawnFullness(float value)
        {
            this.fullness = Mathf.Clamp01(value);  // Ensure fullness is always between 0 and 1
/*
            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"[Symbiophore] Fullness set to {this.fullness} for pawn {parent.LabelShort}");
            }*/
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            var pawn = parent as Pawn;
            if (pawn != null)
            {
                // Now using HediffComp_AnimalEmitter to get MoodProxy
                /* in fact, we have a static method to return this now. Fixme */
                var hediff = pawn.health.hediffSet.hediffs
                    .OfType<HediffWithComps>()
                    .Select(h => h.TryGetComp<HediffComp_AnimalEmitter>())
                    .FirstOrDefault(comp => comp != null);

                if (hediff != null)
                {
                    lastMoodProxy = hediff.MoodProxy;

                   /* if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Message($"CompTickRare: MoodProxy updated to {lastMoodProxy} for {pawn.Name}");
                    }*/
                }

                // Calculate the amount to increment skeinProgress (based on 2 days or gatherResourcesIntervalDays)
                float incrementAmount = SymbiophoreUtility.CalculateSkeinProgressIncrement(GatherResourcesIntervalDays);
                skeinProgress += incrementAmount;
                skeinProgress = Mathf.Clamp01(skeinProgress); // Ensure it doesn't exceed 1.0

                if (skeinProgress >= 1.0f)
                {
                    skeinProgress = 1.0f;  // Clamp to maximum
                    NotifyGamePawnFullness(1.0f);  // Set fullness to 1.0, indicating the animal is fully ready
                }
                     /*   if (SymbiophoreMod.settings.EnableLogging)
                        {
                            Log.Message($"CompTickRare: Skein progress updated for {pawn.Name}. Current progress: {skeinProgress}");
                        }*/
            }
        }

        public float SkeinProgress
          {
              get { return skeinProgress; }
          }

          public void CustomGathered(Pawn doer)
          {
              if (!Active)
              {
                  Log.Error($"{doer} gathered body resources while not Active: {parent}");
              }

              float animalGatherYield = doer.GetStatValue(StatDefOf.AnimalGatherYield);
              if (!Rand.Chance(animalGatherYield))
              {
                  MoteMaker.ThrowText((doer.DrawPos + parent.DrawPos) / 2f, parent.Map, "TextMote_ProductWasted".Translate(), 3.65f);
              }
              else
              {
                  int num = GenMath.RoundRandom((float)ResourceAmount * skeinProgress * AnimalGatherSkillFactor(doer));
                  while (num > 0)
                  {
                      int num2 = Mathf.Clamp(num, 1, ResourceDef.stackLimit);
                      num -= num2;
                      Thing thing = ThingMaker.MakeThing(ResourceDef);
                      thing.stackCount = num2;
                      GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near);
                  }
              }

              // Reset skeinProgress after gathering
              skeinProgress = 0f;

              // Set fullness to zero after gathering is complete
              NotifyGamePawnFullness(0f); // Use the method to set fullness to 0
          }
        private int CalculateResourceAmount()
        {
            var pawn = parent as Pawn;
            if (pawn == null)
            {
                Log.Message($"CalculateResourceAmount: parent is not a pawn. Returning default resource amount: {Props.resourceAmount}");
                return Props.resourceAmount;
            }

            // Now using MoodProxy instead of static moodFactor
            var hediff = pawn.health.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .Select(h => h.TryGetComp<HediffComp_AnimalEmitter>())
                .FirstOrDefault(comp => comp != null);

            float moodProxy = hediff != null ? Mathf.Clamp01(hediff.MoodProxy / 12f) : 1f;

         /*   if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"CalculateResourceAmount: MoodProxy of {pawn.Name}: {hediff?.MoodProxy}, Calculated mood proxy: {moodProxy}");
            }*/

            float healthFactor = 1.0f;
            if (pawn.health != null)
            {
                float healthLevel = pawn.health.summaryHealth.SummaryHealthPercent;
                healthFactor = Mathf.Clamp01(healthLevel);
/*
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"CalculateResourceAmount: Health level of {pawn.Name}: {healthLevel}, Health factor: {healthFactor}");
                }*/
            }

            int calculatedResourceAmount = Mathf.RoundToInt(Props.resourceAmount * moodProxy * healthFactor);
          // Log.Message($"CalculateResourceAmount: Calculated resource amount for {pawn.Name}: {calculatedResourceAmount} (Base amount: {Props.resourceAmount}, Mood proxy: {moodProxy}, Health factor: {healthFactor})");

            return calculatedResourceAmount;
        }

      public new bool ActiveAndFull
      {
          get
          {
              return Active && skeinProgress >= 1f;  // replaces "fullness"
          }
      }

        private float AnimalGatherSkillFactor(Pawn doer)
        {
            if (doer.skills == null || doer.skills.GetSkill(SkillDefOf.Animals) == null)
            {
                return 1.0f;
            }

            int skillLevel = doer.skills.GetSkill(SkillDefOf.Animals).Level;
            float skillFactor = 1.0f + (skillLevel * 0.05f); // 5% to the gather amount

        /*    if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"AnimalGatherSkillFactor: Skill level of {doer.Name}: {skillLevel}, Skill factor: {skillFactor}");
            }*/

            return skillFactor;
        }
    }
}