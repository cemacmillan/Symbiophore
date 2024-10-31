using HarmonyLib;
using Verse;
using UnityEngine;

namespace DIL_Symbiophore
{
    public class SymbiophoreMod : Mod
    {
        public static SymbiophoreModSettings settings;

        public SymbiophoreMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<SymbiophoreModSettings>();
            var harmony = new Harmony("cem.symbiophore");
            harmony.PatchAll();
            Log.Message("<color=#00FF7F>[Symbiophore]</color>v1.5.3 vroomvroomcrash");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            new SymbiophoreModSettingsWindow(settings).DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Symbiophore Mod Settings";
        }
    }
}