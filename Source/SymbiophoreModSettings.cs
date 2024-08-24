using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophoreModSettings : Verse.ModSettings
    {
        public bool EnableLogging = false; // New setting for logging control
        public bool SymbiophoreReproduction = false; // Enable/disable reproduction (not implemented)
        public bool ForceMoodEffect = false; // Force mood effect to 9 (not implemented)

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableLogging, "EnableLogging", false); // Save/Load EnableLogging
            Scribe_Values.Look(ref SymbiophoreReproduction, "SymbiophoreReproduction", false); // Save/Load SymbiophoreReproduction
            Scribe_Values.Look(ref ForceMoodEffect, "ForceMoodEffect", false); // Save/Load ForceMoodEffect
        }
    }
}