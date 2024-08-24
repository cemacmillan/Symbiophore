using Verse;

namespace DIL_Symbiophore
{
    public class Comp_SymbiophorePsychicHarmonizer : ThingComp
    {
        public float moodProxy;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref moodProxy, "moodProxy", 0f);
        }
    }
}

