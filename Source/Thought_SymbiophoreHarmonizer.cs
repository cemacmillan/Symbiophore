using RimWorld;
using UnityEngine;
using Verse;


namespace DIL_Symbiophore
{
    public class Thought_SymbiophoreHarmonizer : Thought_Memory
    {
        public Hediff harmonizer;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref harmonizer, "harmonizer");
        }

        public override float MoodOffset()
        {
            float baseMoodEffect = Mathf.Min(base.MoodOffset(), 1f); // Ensure baseMoodEffect does not exceed 1
            float moodOffset = 12 * baseMoodEffect; // Scale the effect so that it's a value between 0 and 12
            Log.Message($"In MoodOffset(): baseMoodEffect = {baseMoodEffect}, moodOffset = {moodOffset}");
            return moodOffset;
        }






        public override string LabelCap
        {
            get
            {
                if (moodPowerFactor == 0.0f)
                {
                    return this.def.label.CapitalizeFirst();
                }
                else
                {
                    return this.def.label.CapitalizeFirst() + ": x" + this.moodPowerFactor.ToString("0.0");
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
