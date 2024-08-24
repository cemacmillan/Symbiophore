using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    [DefOf]
    public static class DefOfs
    {
        public static ThoughtDef SymbiophoreMoodThought;
        public static HediffDef SymbiophorePsychicHarmonization; // Correct type here

        static DefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfs));
        }
    }
}