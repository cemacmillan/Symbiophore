using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophorePsychicHarmonizer : HediffComp
    {
        private float moodProxy = 3.0f;  // happy symbiophore

        public override void CompPostMake()
        {
            base.CompPostMake();
            // Perform any initialization logic here
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref moodProxy, "moodProxy", 1.0f);  // Serialize/deserialize mood proxy
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

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

            // Check if the symbiophore is outside
            if (!pawn.Position.Roofed(pawn.Map))
            {
                moodProxy += 0.1f;  // Increase mood proxy
            }

            // Check if the symbiophore is eating
            if (pawn.CurJobDef == JobDefOf.Ingest)
            {
                moodProxy += 0.1f;  // Increase mood proxy
            }

            // Check if the symbiophore is hungry
            if (pawn.needs.food.CurLevelPercentage < 0.2f)
            {
                moodProxy -= 0.1f;  // Decrease mood proxy
            }

            // Check if the symbiophore is tired and can't sleep
            if (pawn.needs.rest.CurLevelPercentage < 0.2f && !RestUtility.CanFallAsleep(pawn))
            {
                moodProxy -= 0.1f;  // Decrease mood proxy
            }

            // Check if the symbiophore is in an uncomfortable temperature
            float ambientTemperature = pawn.AmbientTemperature;
            if (ambientTemperature < 21f || ambientTemperature > 37f)
            {
                moodProxy -= 0.1f;  // Decrease mood proxy
            }

            // Ensure mood proxy doesn't go below 0
            moodProxy = Mathf.Max(moodProxy, 0f);

            // Check if the pawn's map or the list of all spawned pawns are null
            if (pawn.Map == null || pawn.Map.mapPawns.AllPawnsSpawned == null)
            {
                return;
            }

            // Affect other pawns with the mood proxy
            List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            AffectPawnsWithMoodProxy(pawn, pawns, moodProxy);
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

                bool hasHarmonizerThought = false;
                foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
                {
                    if (memory is Thought_SymbiophoreHarmonizer thought_SymbiophoreHarmonizer && thought_SymbiophoreHarmonizer.harmonizer == this.parent)
                    {
                        hasHarmonizerThought = true;
                        
                        break;
                    }
                }

                if (!hasHarmonizerThought)
                {
                    Thought_SymbiophoreHarmonizer thought_SymbiophoreHarmonizer2 = (Thought_SymbiophoreHarmonizer)ThoughtMaker.MakeThought(ThoughtDef.Named("SymbiophorePsychicHarmonization"));
                    thought_SymbiophoreHarmonizer2.harmonizer = this.parent;
                    thought_SymbiophoreHarmonizer2.otherPawn = symbiophore;
                    thought_SymbiophoreHarmonizer2.moodPowerFactor = moodProxy; // Modify the thought's mood impact using moodProxy
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought_SymbiophoreHarmonizer2);
                    
                }
            }
        }

    }
}
