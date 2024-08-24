using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class Thought_SymbiophoreHarmonizer : Thought_Memory
    {
        public Hediff harmonizer;
        public float symbiophoreMoodPowerFactor = 1.0f;
        private int lastLogTick = 0; // Track the last tick when the message was logged
        private const int logIntervalTicks = 2500; // Log every 2500 ticks (approx 41.6 in-game seconds)

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref harmonizer, "harmonizer");
            Scribe_Values.Look(ref symbiophoreMoodPowerFactor, "symbiophoreMoodPowerFactor", 1.0f);
        }

        public override float MoodOffset()
        {
            // Use the mood power factor directly for the mood offset, clamping to a maximum of 12
            float moodOffset = Mathf.Min(symbiophoreMoodPowerFactor, 12.0f);

            // Throttle logging to reduce log spam
            if (Find.TickManager.TicksGame - lastLogTick >= logIntervalTicks)
            {
                lastLogTick = Find.TickManager.TicksGame;
                Log.Message($"In MoodOffset(): symbiophoreMoodPowerFactor = {symbiophoreMoodPowerFactor}, moodOffset = {moodOffset}");
            }

            return moodOffset;
        }

        public override string LabelCap
        {
            get
            {
                if (symbiophoreMoodPowerFactor <= 1.0f)
                {
                    return string.Empty;
                }
                else
                {
                    return this.def.label.CapitalizeFirst() + ": x" + this.symbiophoreMoodPowerFactor.ToString("0.0");
                }
            }
        }

        public override bool TryMergeWithExistingMemory(out bool showBubble)
        {
            showBubble = false;
            return false;
        }
    }
}