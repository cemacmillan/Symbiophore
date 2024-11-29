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
            float currentDay = GenLocalDate.DayOfYear(parent.Map);
            if (lastAbsorptionDay != currentDay)
            {
                lastAbsorptionDay = currentDay;
                timesAbsorbedToday = 0;
            }

            // Check if the Symbiophore can absorb again today
            if (timesAbsorbedToday >= maxAbsorptionsPerDay)
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"[Symbiophore] Maximum absorptions reached today for {parent.LabelShort}.");
                }
                return;
            }

            // Check for valid target
            if (targetPawn == null || targetPawn.psychicEntropy == null)
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message("[Symbiophore] Invalid target pawn or target does not have a PsychicEntropyTracker.");
                }
                return;
            }

            // Attempt to drain entropy
            float amountToAbsorb = 1.0f;
            bool success = targetPawn.psychicEntropy.TryAddEntropy(-amountToAbsorb, this.parent, scale: false);
            if (success)
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"[Symbiophore] Drained {amountToAbsorb} entropy from {targetPawn.NameShortColored}. Current entropy: {targetPawn.psychicEntropy.EntropyValue}");
                }

                // Track the number of absorptions for the day
                timesAbsorbedToday++;
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"[Symbiophore] Absorption successful. Times absorbed today: {timesAbsorbedToday}/{maxAbsorptionsPerDay}.");
                }
            }
            else
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"[Symbiophore] Failed to drain entropy from {targetPawn.NameShortColored}. Current entropy: {targetPawn.psychicEntropy.EntropyValue}");
                }
            }
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