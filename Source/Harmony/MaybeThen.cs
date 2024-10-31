using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;

namespace DIL_Symbiophore
{
    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public static class GenerateImpliedDefs_PreResolve_Patch
    {
        public static void Postfix()
        {
            // Define the defName of the symbiophore animal
            string symbiophoreAnimalDefName = "Symbiophore";

            // Locate the ThingDef for the symbiophore animal
            ThingDef symbiophoreAnimalDef = DefDatabase<ThingDef>.GetNamed(symbiophoreAnimalDefName, false);

            if (symbiophoreAnimalDef != null)
            {
                Log.Message("Symbiophore animal def found.");

                // Identify the old Needs tab type
                System.Type oldTabType = typeof(ITab_Pawn_Needs);

                // Find the index of the vanilla Needs tab
                int oldTabIndex = symbiophoreAnimalDef.inspectorTabs.IndexOf(oldTabType);
                if (oldTabIndex >= 0)
                {
                    symbiophoreAnimalDef.inspectorTabs.RemoveAt(oldTabIndex);
                    Log.Message("Old Needs tab found and removed.");
                }
                else
                {
                    Log.Message("Old Needs tab not found.");
                }

                // Add your symbiophore tab at the correct position
                System.Type newTabType = typeof(DIL_Symbiophore.ITab_SymbiophoreNeeds);

                if (oldTabIndex >= 0)
                {
                    symbiophoreAnimalDef.inspectorTabs.Insert(oldTabIndex, newTabType);
                    Log.Message("New Symbiophore tab inserted at old tab position.");
                }
                else
                {
                    symbiophoreAnimalDef.inspectorTabs.Add(newTabType);
                    Log.Message("New Symbiophore tab added to the end.");
                }

                // Dynamically assign custom needs to symbiophore
                symbiophoreAnimalDef.comps.Add(new CompProperties
                {
                    compClass = typeof(CompCustomNeeds)
                });
            }
            else
            {
                Log.Error("Symbiophore animal def not found.");
            }
        }
    }

    // Custom Component to handle needs assignment
    public class CompCustomNeeds : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            // Ensure this is only done for Symbiophore
            if (this.parent.def.defName == "Symbiophore")
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null && pawn.needs != null)
                {
                    pawn.needs.AddOrRemoveNeedsAsAppropriate();
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = this.parent as Pawn;
            if (pawn != null && pawn.needs != null && pawn.def.defName == "Symbiophore")
            {
                // Ensure custom needs are present
                if (!pawn.needs.AllNeeds.Any(n => n.def == CustomNeedDefs.SymbiophoreMood))
                {
                    pawn.needs.AllNeeds.Add(new CustomNeed(pawn) { def = CustomNeedDefs.SymbiophoreMood });
                }
                if (!pawn.needs.AllNeeds.Any(n => n.def == CustomNeedDefs.SymbiophoreFood))
                {
                    pawn.needs.AllNeeds.Add(new CustomNeed(pawn) { def = CustomNeedDefs.SymbiophoreFood });
                }
                if (!pawn.needs.AllNeeds.Any(n => n.def == CustomNeedDefs.SymbiophoreSleep))
                {
                    pawn.needs.AllNeeds.Add(new CustomNeed(pawn) { def = CustomNeedDefs.SymbiophoreSleep });
                }
                if (!pawn.needs.AllNeeds.Any(n => n.def == CustomNeedDefs.SymbiophoreFullness))
                {
                    pawn.needs.AllNeeds.Add(new CustomNeed(pawn) { def = CustomNeedDefs.SymbiophoreFullness });
                }
            }
        }
    }
}