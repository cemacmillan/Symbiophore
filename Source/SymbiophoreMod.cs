using HarmonyLib;
using RimWorld;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophoreMod : Mod
    {
        public SymbiophoreMod(ModContentPack content) : base(content)
        {

            var harmony = new Harmony("cem.symbiophore");
            harmony.PatchAll();
            Log.Message("Symbiophore mod v1.0.3.");
        }
    }
}
