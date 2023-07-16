using RimWorld;
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
            return base.MoodOffset() * moodPowerFactor;
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
