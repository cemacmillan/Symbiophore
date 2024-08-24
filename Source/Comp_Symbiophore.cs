using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class Comp_Symbiophore : ThingComp
    {


        private int timesAbsorbedToday = 0;
        private float lastAbsorptionDay = -1f;
        private const int maxAbsorptionsPerDay = 2; // Maximum absorption times per day

        public Comp_Symbiophore()
        {
           // Log.Message("Comp_Symbiophore instantiated");  // Log when the class is instantiated
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "Absorb Psychic Entropy",
                defaultDesc = "Absorb psychic entropy from another pawn.",
                action = () =>
                {
                    // Start the targeter
                    Find.Targeter.BeginTargeting(new TargetingParameters
                    {
                        canTargetPawns = true, 
                        mapObjectTargetsMustBeAutoAttackable = false,  
                    }, c => AbsorbPsychicEntropyFromPawn(c.Thing as Pawn));  // Call AbsorbPsychicEntropyFromPawn when a target is selected
                },
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"), 
            };
          //  Log.Message("Adding gizmo for " + this.parent.Label); 
            yield return command_Action;
        }


        private void AbsorbPsychicEntropyFromPawn(Pawn targetPawn)
        {
            // Check if it's a new day
            float currentDay = GenDate.DaysPassedFloat;
            if (lastAbsorptionDay != currentDay)
            {
                lastAbsorptionDay = currentDay;
                timesAbsorbedToday = 0;
            }

            // Check if the maximum absorption limit is reached for the day
            if (timesAbsorbedToday >= maxAbsorptionsPerDay)
            {
                return;
            }

            // Check if the target pawn is null or doesn't have psychic entropy
            if (targetPawn == null || targetPawn.psychicEntropy == null)
            {
                return;
            }

            // Absorb psychic entropy from the target pawn
            float amountToAbsorb = 1.0f;  // Replace with the amount of psychic entropy to absorb
            targetPawn.psychicEntropy.TryAddEntropy(-amountToAbsorb, this.parent, false);  // The second parameter is the source of the entropy change, and the third parameter is a label to use in the gizmo that displays the pawn's psychic entropy

            // Increase the mood proxy of the parent pawn
            Comp_SymbiophorePsychicHarmonizer comp = this.parent.GetComp<Comp_SymbiophorePsychicHarmonizer>();
            if (comp != null)
            {
                comp.moodProxy += 0.1f;  // Replace 0.1f with the amount to increase the mood proxy
            }

            // Increase the number of absorptions for the day
            timesAbsorbedToday++;
        }

    }

    public class CompProperties_Symbiophore : CompProperties
    {
        public CompProperties_Symbiophore()
        {
            this.compClass = typeof(Comp_Symbiophore);
        }

        // Define properties for your Comp_Symbiophore class
    }
}
