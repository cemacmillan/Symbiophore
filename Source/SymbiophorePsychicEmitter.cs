using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophorePsychicEmitter : HediffComp
    {
        private int tickCounter = 0;
        private const int inRange = 12 * 12; // symbiophore coverage
        private const int effectInterval = 600; // 600 ticks = 10 seconds in-game
        private float previousMoodProxy = 6.0f;

        public float MoodProxy
        {
            get
            {
                float proxy = MoodProxyManager.GetMoodProxy(this);
              /*  if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"Getting MoodProxy for {this.parent.pawn.Name}: {proxy}");
                }*/
                return proxy;
            }
            set
            {
                MoodProxyManager.SetMoodProxy(this, value);
            /*    if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"Setting MoodProxy for {this.parent.pawn.Name} to {value}");
                }*/
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            MoodProxy = 6.0f;

            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"CompPostMake called, MoodProxy initialized to 6.0 for {this.parent.pawn.Name}.");
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            tickCounter++;
            if (tickCounter < effectInterval)
            {
                return;
            }
            tickCounter = 0;

            if (this.parent == null || this.parent.pawn == null)
            {
                Log.Error("Parent or pawn is null in CompPostTick!");
                return;
            }

            Pawn pawn = this.parent.pawn;

            if (pawn.needs == null || pawn.needs.food == null || pawn.needs.rest == null || pawn.Map == null)
            {
                Log.Error($"Needs or map is null in CompPostTick for {pawn.Name}!");
                return;
            }

            previousMoodProxy = MoodProxy;
            MoodProxy = CalculateSymbiophoreMoodProxy(pawn);

            if (MoodProxy == 1.0f)
            {
                MoodProxy = previousMoodProxy;
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"MoodProxy was 1.0, reset to previous value {previousMoodProxy} for {pawn.Name}.");
                }
            }

            MoodProxy = Mathf.Lerp(previousMoodProxy, MoodProxy, 0.2f);

            if (pawn.Map.mapPawns.AllPawnsSpawned == null)
            {
                Log.Error("AllPawnsSpawned is null in CompPostTick!");
                return;
            }
/*
            if (SymbiophoreMod.settings.EnableLogging && Mathf.Abs(MoodProxy - previousMoodProxy) > 1.0f)
            {
                Log.Message($"MoodProxy changed from {previousMoodProxy} to {MoodProxy} for {pawn.Name} (Temperature: {pawn.AmbientTemperature}, Rain: {pawn.Map.weatherManager.RainRate}, Unroofed: {!pawn.Position.Roofed(pawn.Map)}, Food: {pawn.needs.food.CurLevelPercentage}, Rest: {pawn.needs.rest.CurLevelPercentage})");
            }*/

            List<Pawn> pawns = (List<Pawn>)pawn.Map.mapPawns.AllPawnsSpawned;
            AffectPawnsWithMoodProxy(pawn, pawns, MoodProxy);
        }

        private float CalculateSymbiophoreMoodProxy(Pawn pawn)
        {
            float moodProxy = 0.0f;

            // if (SymbiophoreMod.settings.EnableLogging)
            // {
            //     Log.Message($"Calculating SymbiophoreMoodProxy for {pawn.Name}");
            // }

            float ambientTemperature = pawn.AmbientTemperature;
            if (ambientTemperature < 16f)
            {
                moodProxy = -3f;
            }
            else if (ambientTemperature >= 16f && ambientTemperature < 19f)
            {
                moodProxy = (ambientTemperature - 16f) / 3f * 1f;
            }
            else if (ambientTemperature >= 19f && ambientTemperature < 37f)
            {
                moodProxy = 1f + (ambientTemperature - 19f) / 18f * 8f;
            }
            else if (ambientTemperature >= 37f && ambientTemperature < 45f)
            {
                moodProxy = 9f;
            }
            else if (ambientTemperature >= 45f && ambientTemperature < 53f)
            {
                moodProxy = 9f - (ambientTemperature - 45f) / 8f * 9f;
            }
            else
            {
                moodProxy = -3f;
            }

            if (pawn.Map.weatherManager.RainRate > 0.1f)
            {
                moodProxy += 3f;
            }

            if (!pawn.Position.Roofed(pawn.Map))
            {
                moodProxy *= 1.25f;
            }

            if (pawn.needs.food.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            if (pawn.needs.rest.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            moodProxy = Mathf.Clamp(moodProxy, -3f, 12f);
/*
            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"Calculated moodProxy: {moodProxy} for {pawn.Name} (Temperature: {ambientTemperature}, Rain: {pawn.Map.weatherManager.RainRate}, Unroofed: {!pawn.Position.Roofed(pawn.Map)}, Food: {pawn.needs.food.CurLevelPercentage})");
            }*/

            return moodProxy;
        }

        private void AffectPawnsWithMoodProxy(Pawn symbiophore, List<Pawn> pawns, float moodProxy)
        {
            if (symbiophore == null || pawns == null || symbiophore.Downed)
            {
                Log.Error("Symbiophore, pawns, or symbiophore is downed in AffectPawnsWithMoodProxy!");
                return;
            }

            foreach (Pawn pawn in pawns)
            {
                if (pawn != symbiophore && !pawn.Downed && pawn.Map == symbiophore.Map)
                {
                    if (pawn.Position.DistanceToSquared(symbiophore.Position) <= inRange)
                    {
                        if (pawn.needs?.mood != null)
                        {
                            Thought_SymbiophoreEmitter existingEmitterThought = (Thought_SymbiophoreEmitter)pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(DIL_Symbiophore.DefOfs.SymbiophoreMoodThought);
                            if (existingEmitterThought == null)
                            {
                                Thought_SymbiophoreEmitter newEmitterThought = (Thought_SymbiophoreEmitter)ThoughtMaker.MakeThought(DIL_Symbiophore.DefOfs.SymbiophoreMoodThought);
                                newEmitterThought.symbiophoreMoodPowerFactor = moodProxy;
                                pawn.needs.mood.thoughts.memories.TryGainMemory(newEmitterThought);
/*
                                if (SymbiophoreMod.settings.EnableLogging)
                                {
                                    Log.Message($"Adding new emitter thought to {pawn.Name}");
                                }*/
                            }
                            else
                            {
                                float targetMoodPowerFactor = moodProxy;
                                if (targetMoodPowerFactor < 0 || targetMoodPowerFactor > 12)
                                {
                                    if (SymbiophoreMod.settings.EnableLogging)
                                    {
                                        Log.Warning($"Unexpected symbiophoreMoodPowerFactor: {targetMoodPowerFactor} for {pawn.Name}. Clamping to valid range.");
                                    }
                                    targetMoodPowerFactor = Mathf.Clamp(targetMoodPowerFactor, 0, 12);
                                }
                                existingEmitterThought.symbiophoreMoodPowerFactor = targetMoodPowerFactor;

                               /* if (SymbiophoreMod.settings.EnableLogging)
                                {
                                    Log.Message($"Updated existing emitter thought for {pawn.Name} with symbiophoreMoodPowerFactor = {targetMoodPowerFactor}");
                                }*/
                            }
                        }
                        else
                        {
                            Log.Error($"Pawn {pawn.Name} has no mood or mood need is null.");
                        }
                    }
                }
            }
        }
    }
}