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
            get => MoodProxyManager.GetMoodProxy(this);
            set => MoodProxyManager.SetMoodProxy(this, value);
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            MoodProxy = 6.0f;
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

            previousMoodProxy = MoodProxy;
            MoodProxy = CalculateSymbiophoreMoodProxy(pawn);

            // Ignore changes to exactly 1
            if (MoodProxy == 1.0f)
            {
                MoodProxy = previousMoodProxy;
            }

            // Smoothing adjustment
            MoodProxy = Mathf.Lerp(previousMoodProxy, MoodProxy, 0.2f);

            // Check if the list of all spawned pawns are null
            if (pawn.Map.mapPawns.AllPawnsSpawned == null)
            {
                return;
            }

            // Log the moodProxy change if it deviates significantly and logging is enabled
            if (SymbiophoreMod.settings.EnableLogging && Mathf.Abs(MoodProxy - previousMoodProxy) > 1.0f)
            {
                Log.Message($"MoodProxy changed from {previousMoodProxy} to {MoodProxy} for {pawn.Name} (Temperature: {pawn.AmbientTemperature}, Rain: {pawn.Map.weatherManager.RainRate}, Unroofed: {!pawn.Position.Roofed(pawn.Map)}, Food: {pawn.needs.food.CurLevelPercentage}, Rest: {pawn.needs.rest.CurLevelPercentage})");
            }

            // Affect other pawns with the mood proxy
            List<Pawn> pawns = (List<Pawn>)pawn.Map.mapPawns.AllPawnsSpawned;
            AffectPawnsWithMoodProxy(pawn, pawns, MoodProxy);
        }

        private void AffectPawnsWithMoodProxy(Pawn symbiophore, List<Pawn> pawns, float moodProxy)
        {
            if (symbiophore == null || pawns == null || symbiophore.Downed)
            {
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
                            Thought_SymbiophoreHarmonizer existingHarmonizerThought = (Thought_SymbiophoreHarmonizer)pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(DIL_Symbiophore.DefOfs.SymbiophoreMoodThought);
                            if (existingHarmonizerThought == null)
                            {
                                Thought_SymbiophoreHarmonizer newHarmonizerThought = (Thought_SymbiophoreHarmonizer)ThoughtMaker.MakeThought(DIL_Symbiophore.DefOfs.SymbiophoreMoodThought);
                                newHarmonizerThought.symbiophoreMoodPowerFactor = moodProxy; // Directly use moodProxy
                                pawn.needs.mood.thoughts.memories.TryGainMemory(newHarmonizerThought);
                                if (SymbiophoreMod.settings.EnableLogging)
                                {
                                    Log.Message($"Adding new harmonizer thought to {pawn.Name}");
                                }
                            }
                            else
                            {
                                // Directly set the mood power factor to reflect the current mood proxy
                                float targetMoodPowerFactor = moodProxy;
                                if (targetMoodPowerFactor < 0 || targetMoodPowerFactor > 12)
                                {
                                    if (SymbiophoreMod.settings.EnableLogging)
                                    {
                                        Log.Warning($"Unexpected symbiophoreMoodPowerFactor: {targetMoodPowerFactor} for {pawn.Name}. Clamping to valid range.");
                                    }
                                    targetMoodPowerFactor = Mathf.Clamp(targetMoodPowerFactor, 0, 12);
                                }
                                existingHarmonizerThought.symbiophoreMoodPowerFactor = targetMoodPowerFactor;
                                if (SymbiophoreMod.settings.EnableLogging)
                                {
                                    Log.Message($"Updated existing harmonizer thought for {pawn.Name} with symbiophoreMoodPowerFactor = {targetMoodPowerFactor}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private float CalculateSymbiophoreMoodProxy(Pawn pawn)
        {
            float baseMoodProxy = 0.0f;
            float moodProxy = baseMoodProxy;

            // Temperature Contribution
            float ambientTemperature = pawn.AmbientTemperature;
            if (ambientTemperature < 16f)
            {
                moodProxy = -3f; // Hypothermic range, no emanation
            }
            else if (ambientTemperature >= 16f && ambientTemperature < 19f)
            {
                // Increase linearly from 16°C to 19°C
                moodProxy = (ambientTemperature - 16f) / 3f * 1f;
            }
            else if (ambientTemperature >= 19f && ambientTemperature < 37f)
            {
                // Increase linearly from 19°C to 37°C, capping at +9
                moodProxy = 1f + (ambientTemperature - 19f) / 18f * 8f;
            }
            else if (ambientTemperature >= 37f && ambientTemperature < 45f)
            {
                moodProxy = 9f; // Peak mood proxy
            }
            else if (ambientTemperature >= 45f && ambientTemperature < 53f)
            {
                // Decrease linearly from 45°C to 53°C
                moodProxy = 9f - (ambientTemperature - 45f) / 8f * 9f;
            }
            else
            {
                moodProxy = -3f; // Too hot, no emanation
            }

            // Rain Contribution
            if (pawn.Map.weatherManager.RainRate > 0.1f)
            {
                moodProxy += 3f;
            }

            // Unroofed Contribution
            if (!pawn.Position.Roofed(pawn.Map))
            {
                moodProxy *= 1.25f;
            }

            // Hunger Penalty
            if (pawn.needs.food.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            // Sleep Penalty
            if (pawn.needs.rest.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            // Clamp the moodProxy to the desired range
            moodProxy = Mathf.Clamp(moodProxy, -3f, 12f);

            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"Calculated moodProxy: {moodProxy} for pawn: {pawn.Name} (Temperature: {ambientTemperature}, Rain: {pawn.Map.weatherManager.RainRate}, Unroofed: {!pawn.Position.Roofed(pawn.Map)}, Food: {pawn.needs.food.CurLevelPercentage})");
            }

            return moodProxy;
        }
    }
}