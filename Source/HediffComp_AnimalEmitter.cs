
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class HediffComp_AnimalEmitter : HediffComp, ILoadReferenceable
    {
        public HediffCompProperties_AnimalEmitter Props => (HediffCompProperties_AnimalEmitter)props;

        private const int effectInterval = 3600; // was 600
        private float previousMoodProxy = 6.0f;

        // Expose the mood proxy value for use in other parts of the mod
        public float MoodProxy
        {
            get { return moodProxy; }
            set 
            {
                moodProxy = value;
         /*       if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"MoodProxy set to {moodProxy} for {parent?.pawn?.LabelShort ?? "Unknown Pawn"}");
                }*/
            }
        }

        private float moodProxy;

        // Expose data for saving/loading
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref moodProxy, "MoodProxy", 6.0f);

           /* if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"CompExposeData called for {parent?.pawn?.LabelShort ?? "Unknown Pawn"}. MoodProxy: {MoodProxy}");
            }*/
        }

            public override void CompPostTick(ref float severityAdjustment)
            {
                Pawn pawn = parent?.pawn;

                // Ensure the pawn is valid and alive
                if (pawn == null || pawn.Dead || pawn.health == null || pawn.Map == null)
                {
                    /*  Not useful except in debugging

                      if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Message($"Pawn {pawn?.LabelShort ?? "Unknown Pawn"} is either dead or invalid, stopping CompPostTick.");
                    }*/

                    // Stop further ticks since the pawn is invalid or dead
                    return;
                }

                // Implement rare tick behavior by only running every 250 ticks (4.17 seconds in-game)
                if (pawn.IsHashIntervalTick(250))
                {
                    // mood proxy calculation in Utilities class
                    previousMoodProxy = MoodProxy;
                    MoodProxy = SymbiophoreUtility.CalculateMoodProxy(pawn);

                    // Smooth mood proxy to avoid abrupt changes
                    MoodProxy = Mathf.Lerp(previousMoodProxy, MoodProxy, 0.2f);
/*
                    if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Message($"MoodProxy updated for {pawn.LabelShort}: {MoodProxy}");
                    }
*/
                    // Affect nearby pawns with the mood proxy
                    List<Pawn> nearbyPawns = (List<Pawn>) pawn.Map.mapPawns.AllPawnsSpawned;
                    AffectPawnsWithMoodProxy(pawn, nearbyPawns, MoodProxy);
                }
            }
        private void AffectPawnsWithMoodProxy(Pawn emitter, List<Pawn> pawns, float moodProxy)
        {
            foreach (Pawn pawn in pawns)
            {
                // distance and humanlike. these tests are backwards and should be distance-squared based
                if (pawn != emitter && pawn.Position.DistanceTo(emitter.Position) <= Props.range && pawn.RaceProps.Humanlike)
                {
                    // Apply the mood effect to the target pawn
                    ApplyEffect(emitter, pawn, MoodProxy);
                }
            }
        }

        private void ApplyEffect(Pawn emitterPawn, Pawn targetPawn, float moodProxy)
        {
            ThoughtDef thoughtDef = Props.thought;
            if (thoughtDef != null)
            {
             /*   if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"Applying effect to {targetPawn.LabelShort}. MoodProxy: {moodProxy}");
                }
*/
                // Create the thought
                Thought_SymbiophoreEmitter memory = (Thought_SymbiophoreEmitter)ThoughtMaker.MakeThought(thoughtDef);

                // get emitter from the pawn
                var assignedEmitter = SymbiophoreUtility.GetAnimalEmitter(emitterPawn);  // Ensure emitterPawn's emitter is assigned

                // attach the emitter to the memory
                memory.SetEmitter(assignedEmitter);

             /*   if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"Assigned emitter to thought. Emitter pawn: {memory.emitter?.parent?.pawn?.LabelShort ?? "NULL"}");
                }
*/
                // moodPowerFactor is now moodProxy, since the latter is already well cooked
                memory.moodPowerFactor = moodProxy;

                // Don't bother with zero.
                if (memory.moodPowerFactor > 0.01f)
                {
                    targetPawn.needs.mood.thoughts.memories.TryGainMemory(memory);

              /*      if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Message($"Effect applied to {targetPawn.LabelShort}: {memory.def.defName}");
                    }*/
                }
                   /* - just let this fall through. it's not an intereting condition. 
                else
                {
                if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Message($"Effect not applied to {targetPawn.LabelShort} because MoodPowerFactor is too low.");
                    }
                }*/
            }
        }

        // May need to do something more complex here, perhaps registering all emitter LoadId o the map
        public string GetUniqueLoadID()
        {
            return $"HediffComp_AnimalEmitter_{parent?.GetUniqueLoadID() ?? "Unknown"}";
        }
    }

    public class HediffCompProperties_AnimalEmitter : HediffCompProperties
    {
        public float range = 10f;
        public ThoughtDef thought;
        public float moodFactor = 1f;

        public HediffCompProperties_AnimalEmitter()
        {
            this.compClass = typeof(HediffComp_AnimalEmitter);
        }
    }
    
}