using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophorePsychicHarmonizer : HediffComp
    {
        private int tickCounter = 0;
        public float moodProxy = 6.0f;

        public override void CompPostMake()
        {
            base.CompPostMake();
            moodProxy = 6.0f;
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

            // Check if the pawn's needs, food need, or rest need are null
            if (pawn.needs == null || pawn.needs.food == null || pawn.needs.rest == null || pawn.Map == null)
            {
                return;
            }

            moodProxy = CalculateSymbiophoreMoodProxy(pawn);
            //Log.Message($"Mood proxy after calculation for pawn {pawn.Name}: {moodProxy}");

            // Check if the list of all spawned pawns are null
            if (pawn.Map.mapPawns.AllPawnsSpawned == null)
            {
                return;
            }

            // Affect other pawns with the mood proxy
            List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            AffectPawnsWithMoodProxy(pawn, pawns, moodProxy);
        }

        private void AffectPawnsWithMoodProxy(Pawn symbiophore, List<Pawn> pawns, float moodProxy)
        {
            if (symbiophore == null || pawns == null || symbiophore.Downed || symbiophore.MentalState != null || symbiophore.Dead)
            {
                return;
            }

            foreach (Pawn pawn in pawns)
            {
                if (pawn == null || symbiophore == pawn || !pawn.RaceProps.Humanlike || pawn.needs?.mood?.thoughts == null || pawn.Position.DistanceTo(symbiophore.Position) > 30)
                {
                    continue;
                }

                Thought_SymbiophoreHarmonizer existingHarmonizerThought = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DIL_Symbiophore.DefOfs.SymbiophorePsychicHarmonization) as Thought_SymbiophoreHarmonizer;

                if (existingHarmonizerThought == null)
                {
                    Thought_SymbiophoreHarmonizer thought_SymbiophoreCaster = (Thought_SymbiophoreHarmonizer)ThoughtMaker.MakeThought(DIL_Symbiophore.DefOfs.SymbiophorePsychicHarmonization);
                    thought_SymbiophoreCaster.harmonizer = this.parent;
                    thought_SymbiophoreCaster.otherPawn = symbiophore;
                    thought_SymbiophoreCaster.moodPowerFactor = Mathf.Clamp(moodProxy * 1.2f, -3f, 12f);

                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought_SymbiophoreCaster);
                }
                else
                {
                    float targetMoodPowerFactor = Mathf.Clamp(moodProxy * 1.2f, -3f, 12f);
                    existingHarmonizerThought.moodPowerFactor = Mathf.Lerp(existingHarmonizerThought.moodPowerFactor, targetMoodPowerFactor, 0.2f);
                }
            }
        }


        private float CalculateSymbiophoreMoodProxy(Pawn pawn)
        {
            float moodProxy = 6.0f;

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
                moodProxy *= 0.5f;
            }

            // Check if the symbiophore is tired and can't sleep
            if (pawn.needs.rest.CurLevelPercentage < 0.2f && !RestUtility.CanFallAsleep(pawn))
            {
                moodProxy *= 0.8f;
            }

            // Check if the symbiophore is in an uncomfortable temperature
            float ambientTemperature = pawn.AmbientTemperature;
            if (ambientTemperature < 21f || ambientTemperature > 37f)
            {
                moodProxy *= 0.8f;
            }

            // Check if the symbiophore is in pain
            float painTotal = pawn.health.hediffSet.PainTotal;
            if (painTotal > 0)
            {
                moodProxy *= 1 - painTotal;  // adjust the mood depending on the amount of pain. 
            }

            // Check if the symbiophore is on fire
            if (pawn.IsBurning())
            {
                moodProxy *= 0;
            }

            moodProxy = Mathf.Clamp(moodProxy, -3f, 12f);

            return moodProxy;
        }

    }
}
