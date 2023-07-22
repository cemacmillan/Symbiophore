using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    [DefOf]
    public static class DefOfs
    {
        public static ThoughtDef SymbiophorePsychicHarmonization;

        static DefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfs));
        }
    }
}



