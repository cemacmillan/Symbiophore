using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class Comp_Symbiophore : ThingComp
    {
        public Comp_Symbiophore()
        {
           // Log.Message("Comp_Symbiophore instantiated");  // Log when the class is instantiated
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
           // Log.Message("CompGetGizmosExtra called for " + this.parent.Label);  // Log when the method is called

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
                        canTargetPawns = true,  // We want to target pawns
                        mapObjectTargetsMustBeAutoAttackable = false,  // We don't need to be able to attack the target
                    }, c => AbsorbPsychicEntropyFromPawn(c.Thing as Pawn));  // Call AbsorbPsychicEntropyFromPawn when a target is selected
                },
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"),  // Replace with your own texture
            };
          //  Log.Message("Adding gizmo for " + this.parent.Label);  // Log when the gizmo is added
            yield return command_Action;
        }


        private void AbsorbPsychicEntropyFromPawn(Pawn targetPawn)
        {
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
