using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;

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
            // Identify the old Needs tab type
            System.Type oldTabType = typeof(ITab_Pawn_Needs); 
            
            // Find and remove the vanilla Needs tab
            InspectorTabDef oldTab = symbiophoreAnimalDef.inspectorTabs.FirstOrDefault(tab => tab.inspectorTabClass == oldTabType);
            if (oldTab != null)
            {
                symbiophoreAnimalDef.inspectorTabs.Remove(oldTab);
            }

            // Add the custom symbiophore tab at the correct position
            InspectorTabDef newTab = DefDatabase<InspectorTabDef>.GetNamed("DIL_Symbiophore.ITab_SymbiophoreNeeds", false);
            if (newTab != null)
            {
                // Insert the new tab at the old tab's position if the old tab was found
                if (oldTab != null)
                {
                    int oldTabIndex = symbiophoreAnimalDef.inspectorTabs.IndexOf(oldTab);
                    symbiophoreAnimalDef.inspectorTabs.Insert(oldTabIndex, newTab);
                }
                else
                {
                    // If the old tab was not found, add the new tab to the end
                    symbiophoreAnimalDef.inspectorTabs.Add(newTab);
                }
            }
        }
    }
}