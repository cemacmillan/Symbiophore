using Verse;

namespace DIL_Symbiophore
{
    public class CompProperties_Skeinable : CompProperties
    {
        public int gatherResourcesIntervalDays;
        public int resourceAmount;
        public ThingDef resourceDef;

        public CompProperties_Skeinable()
        {
            this.compClass = typeof(CompSkeinable);
        }
    }
}