using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    [DefOf]
    public static class DefOfs
    {
        public static NeedDef Mood;

        static DefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfs));
            Mood = DefOf_Mood.Mood;
        }
    }
}


