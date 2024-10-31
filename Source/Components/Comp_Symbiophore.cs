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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (parent is Pawn pawn && pawn.def.defName == "Symbiophore" && !pawn.Dead)
            {
                // Ensure the Hediff is applied
                if (!pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation")))
                {
                    HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation");
                    if (hediffDef != null)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        pawn.health.AddHediff(hediff, pawn.health.hediffSet.GetBrain());
                    }
                    else
                    {
                        Log.Error("Failed to find HediffDef 'SymbiophorePsychicEmanation'.");
                    }
                }
            }
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
            yield return command_Action;
        }

        private void AbsorbPsychicEntropyFromPawn(Pawn targetPawn)
        {
            float currentDay = GenDate.DaysPassedFloat;
            if (lastAbsorptionDay != currentDay)
            {
                lastAbsorptionDay = currentDay;
                timesAbsorbedToday = 0;
            }

            // Can this pawn absorb again today?
            if (timesAbsorbedToday >= maxAbsorptionsPerDay)
            {
                return;
            }

            if (targetPawn == null || targetPawn.psychicEntropy == null)
            {
                return;
            }

            // Absorb psychic entropy
            float amountToAbsorb = 1.0f; // Amount to absorb
            targetPawn.psychicEntropy.TryAddEntropy(-amountToAbsorb, this.parent, false);  // Negative value to reduce entropy

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
    }
}