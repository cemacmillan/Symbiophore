using Verse;

namespace DIL_Symbiophore
{
    public class CompProperties_CustomSkeinable : CompProperties
    {
        public int gatherResourcesIntervalDays;
        public int resourceAmount;
        public ThingDef resourceDef;

        public CompProperties_CustomSkeinable()
        {
            this.compClass = typeof(CompCustomSkeinable);
        }
    }
}