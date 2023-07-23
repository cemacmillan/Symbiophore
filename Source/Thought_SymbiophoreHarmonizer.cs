using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class Thought_SymbiophoreHarmonizer : Thought_Memory
    {
        public Hediff harmonizer;
        public float symbiophoresymbiophoreMoodPowerFactor = 1.0f;  // Adding symbiophoresymbiophoreMoodPowerFactor as a field

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref harmonizer, "harmonizer");
            Scribe_Values.Look(ref symbiophoresymbiophoreMoodPowerFactor, "symbiophoresymbiophoreMoodPowerFactor", 1.0f); // Ensure symbiophoresymbiophoreMoodPowerFactor is saved
        }

        public override float MoodOffset()
        {
            float moodOffset = 12 * Mathf.Min(moodPowerFactor / 10f, 1f);
            //Log.Message($"In MoodOffset(): moodPowerFactor = {moodPowerFactor}, moodOffset = {moodOffset}");
            return moodOffset;
        }


        public override string LabelCap
        {
            get
            {
                if (symbiophoresymbiophoreMoodPowerFactor == 0.0f)
                {
                    return this.def.label.CapitalizeFirst();
                }
                else
                {
                    return this.def.label.CapitalizeFirst() + ": x" + this.symbiophoresymbiophoreMoodPowerFactor.ToString("0.0");
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
