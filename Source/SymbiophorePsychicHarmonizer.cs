using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophorePsychicHarmonizer : HediffComp
    {
        private int tickCounter = 0;
        public float moodProxy = 1.0f;

        public override void CompPostMake()
        {
            base.CompPostMake();
            moodProxy = 1.0f;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            tickCounter++;
            if (tickCounter < 600)  // Only apply the mood effect every 600 ticks (1 real-time second at normal speed)
            {
                return;
            }
            tickCounter = 0;

            // Check if the parent or the pawn are null
            if (this.parent == null || this.parent.pawn == null)
            {
                return;
            }

            Pawn pawn = this.parent.pawn;

            // Check if the pawn's map, needs, food need, or rest need are null
            if (pawn.Map == null || pawn.needs == null || pawn.needs.food == null || pawn.needs.rest == null)
            {
                return;
            }

            // Reset moodProxy to base value
            moodProxy = 1.0f;

            // Check if the symbiophore is outside
            if (!pawn.Position.Roofed(pawn.Map))
            {
                moodProxy *= 1.1f;  
            }

            // Check if the symbiophore is eating
            if (pawn.CurJobDef == JobDefOf.Ingest)
            {
                moodProxy *= 1.1f;  
            }

            // Check if the symbiophore is hungry
            if (pawn.needs.food.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.8f;  
            }

            // Check if the symbiophore is tired and can't sleep
            if (pawn.needs.rest.CurLevelPercentage < 0.2f && !RestUtility.CanFallAsleep(pawn))
            {
                moodProxy *= 0.9f;  
            }

            // Check if the symbiophore is in an uncomfortable temperature
            float ambientTemperature = pawn.AmbientTemperature;
            if (ambientTemperature < 21f || ambientTemperature > 37f)
            {
                moodProxy *= 0.8f;  // Decrease mood proxy by 20%
            }

            float assumedNeutralMood = 0.5f; // Assuming mood is on a scale from 0 to 1
            moodProxy = moodProxy / (1.0f + (1 - assumedNeutralMood));

            moodProxy = Mathf.Max(moodProxy, 0f);
            // Check if the pawn's map or the list of all spawned pawns are null
            if (pawn.Map == null || pawn.Map.mapPawns.AllPawnsSpawned == null)
            {
                return;
            }

            // Affect other pawns with the mood proxy
            List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            // Because I have seen a value of thousands
            float clampedMoodProxy = Mathf.Min(moodProxy, 10f);
            AffectPawnsWithMoodProxy(pawn, pawns, clampedMoodProxy);
        }
        private void AffectPawnsWithMoodProxy(Pawn symbiophore, List<Pawn> pawns, float moodProxy)
        {
            if (symbiophore == null || pawns == null)
            {
                return;
            }

            foreach (Pawn pawn in pawns)
            {
                if (pawn == null || symbiophore == pawn || !pawn.RaceProps.Humanlike || pawn.needs?.mood?.thoughts == null || pawn.Position.DistanceTo(symbiophore.Position) > 30)
                {
                    continue;
                }

                // Only apply the effect if the pawn is not downed, not in a mental state, and not dead.
                if (!symbiophore.Downed && symbiophore.MentalState == null && !symbiophore.Dead)
                {
                    Thought_SymbiophoreHarmonizer existingHarmonizerThought = null;
                    foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
                    {
                        if (memory is Thought_SymbiophoreHarmonizer thought_SymbiophoreHarmonizer && thought_SymbiophoreHarmonizer.harmonizer == this.parent)
                        {
                            existingHarmonizerThought = thought_SymbiophoreHarmonizer;
                            break;
                        }
                    }

                    if (existingHarmonizerThought == null)
                    {
                        Thought_SymbiophoreHarmonizer thought_SymbiophoreHarmonizer2 = (Thought_SymbiophoreHarmonizer)ThoughtMaker.MakeThought(ThoughtDef.Named("SymbiophorePsychicHarmonization"));
                        thought_SymbiophoreHarmonizer2.harmonizer = this.parent;
                        thought_SymbiophoreHarmonizer2.otherPawn = symbiophore;
                        thought_SymbiophoreHarmonizer2.moodPowerFactor = Mathf.Min(moodProxy, 10f); // Normalize moodProxy and clamp the moodPowerFactor

                        pawn.needs.mood.thoughts.memories.TryGainMemory(thought_SymbiophoreHarmonizer2);

                        // Log the mood effect being applied
                        //Log.Message($"Applying mood effect to pawn {pawn.Name}: moodProxy = {moodProxy}, moodPowerFactor = {thought_SymbiophoreHarmonizer2.moodPowerFactor}");
                    }
                    else
                    {
                        float targetMoodPowerFactor = Mathf.Min(moodProxy * 1.2f, 12f); // moodProxy normalized to 1.2 times its value and clamped at 12
                        existingHarmonizerThought.moodPowerFactor = Mathf.Lerp(existingHarmonizerThought.moodPowerFactor, targetMoodPowerFactor, 0.1f);

                       // Log.Message($"In AffectPawnsWithMoodProxy(): moodProxy = {moodProxy}, moodPowerFactor = {existingHarmonizerThought.moodPowerFactor}");
                    }
                }
            }
        }



    }
}
