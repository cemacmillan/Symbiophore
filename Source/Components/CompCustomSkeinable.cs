using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace DIL_Symbiophore
{
    public class CompCustomSkeinable : CompHasGatherableBodyResource
    {
        protected override int GatherResourcesIntervalDays => Props.gatherResourcesIntervalDays;
        protected override int ResourceAmount => CalculateResourceAmount();
        protected override ThingDef ResourceDef => Props.resourceDef;
        protected override string SaveKey => "skeinProgress";
        protected float lastMoodProxy;

        public CompProperties_CustomSkeinable Props => (CompProperties_CustomSkeinable)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent.IsHashIntervalTick(2500)) 
            {
              
                var pawn = parent as Pawn;
                if (pawn != null)
                {
                    // verify
                    var hediff = pawn.health.hediffSet.hediffs.OfType<SymbiophorePsychicEmitter>().FirstOrDefault();
                    if (hediff != null)
                    {
                        lastMoodProxy = hediff.MoodProxy;
                    }
                }
            }
        }

        // Generally applies to AARF
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
                int num = GenMath.RoundRandom((float)ResourceAmount * fullness * AnimalGatherSkillFactor(doer));
                while (num > 0)
                {
                    int num2 = Mathf.Clamp(num, 1, ResourceDef.stackLimit);
                    num -= num2;
                    Thing thing = ThingMaker.MakeThing(ResourceDef);
                    thing.stackCount = num2;
                    GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near);
                }
            }
            fullness = 0f;
        }

        // Generally applies to AARF
        private int CalculateResourceAmount()
        {
            var pawn = parent as Pawn;
            if (pawn == null)
            {
                Log.Message($"CalculateResourceAmount: parent is not a pawn. (This is not normal - please signal as a bug if possible) Returning default resource amount: {Props.resourceAmount}.");
                return Props.resourceAmount;
            }

            // 
            var hediff = pawn.health.hediffSet.hediffs.OfType<SymbiophorePsychicEmitter>().FirstOrDefault();
            float moodFactor = hediff != null ? Mathf.Clamp01(hediff.MoodProxy / 12f) : 1f;

                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"CalculateResourceAmount: Mood proxy of {pawn.Name}: {hediff?.MoodProxy}, Mood factor: {moodFactor}");
                }

            float healthFactor = 1.0f;
            if (pawn.health != null)
            {
                float healthLevel = pawn.health.summaryHealth.SummaryHealthPercent;
                healthFactor = Mathf.Clamp01(healthLevel);

                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"CalculateResourceAmount: Health level of {pawn.Name}: {healthLevel}, Health factor: {healthFactor}");
                }
            }

            int calculatedResourceAmount = Mathf.RoundToInt(Props.resourceAmount * moodFactor * healthFactor);
            Log.Message($"CalculateResourceAmount: Calculated resource amount for {pawn.Name}: {calculatedResourceAmount} (Base amount: {Props.resourceAmount}, Mood factor: {moodFactor}, Health factor: {healthFactor})");

            return calculatedResourceAmount;
        }

        // AARF method
        private float AnimalGatherSkillFactor(Pawn doer)
        {
            if (doer.skills == null || doer.skills.GetSkill(SkillDefOf.Animals) == null)
            {
                return 1.0f; // But, this is messed up
            }

            int skillLevel = doer.skills.GetSkill(SkillDefOf.Animals).Level;
            float skillFactor = 1.0f + (skillLevel * 0.05f); // 5% to the gather amount.. maybe instead augment gather amount 
           
            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"AnimalGatherSkillFactor: Skill level of {doer.Name}: {skillLevel}, Skill factor: {skillFactor}");
            }

            return skillFactor;
        }
    }
}