using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace DIL_Symbiophore
{
    public static class SymbiophoreUtility
    {
        // Constants for temperature ranges and bonuses
        private const float IdealMinTemp = 28f;
        private const float IdealMaxTemp = 37f;
        private const float ComfortMinTemp = 13f;
        private const float ComfortMaxTemp = 49f;
        private const float SevereColdTemp = 7f;
        private const float SevereHeatTemp = 50f;
        private const float EmergencyFallbackValue = 47f;

        private static HashSet<Pawn> loggedPawns = new HashSet<Pawn>();

        public static bool CanAccumulate(Pawn pawn, bool logErrorOnce = false)
        {
            // Check if the pawn is null
            if (pawn == null)
            {
                return false;
            }

            // Check if the pawn is dead
            if (pawn.Dead)
            {
                return false;
            }

            // Check if the pawn is unconscious
            if (!pawn.health.capacities.CanBeAwake)
            {
                return false;
            }

            if (!pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation")))
            {
                if (logErrorOnce && !loggedPawns.Contains(pawn))
                {
                    Log.Error($"[Symbiophore] Pawn {pawn.LabelShort} does not have the required Symbiophore hediff.");
                    loggedPawns.Add(pawn);
                }
                return false;
            }

            return true;
        }


        // this method increments on RareTick, so the math is unusual
        public static float CalculateSkeinProgressIncrement(int gatherResourcesIntervalDays)
        {
          
            return 1f / (gatherResourcesIntervalDays * 60000f / 250f);  
        }

        public static bool CanEmit(Pawn pawn, bool logErrorOnce = false)
        {
           
            if (pawn == null)
            {
                return false;
            }

            if (pawn.Dead)
            {
                return false;
            }

            if (!pawn.health.capacities.CanBeAwake)
            {
                return false;
            }

            // If the pawn isn't a Symbiophore (or doesn't have the necessary hediff), log an error once
            if (!pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation")))
            {
                if (logErrorOnce && !loggedPawns.Contains(pawn))
                {
                    Log.Error($"[Symbiophore] Pawn {pawn.LabelShort} does not have the required Symbiophore hediff.");
                    loggedPawns.Add(pawn);  // Add to the set so we don't log again for this pawn
                }
                return false;
            }

            return true;
        }

        public static void ClearLoggedPawns()
        {
            // Reset the log once in case you need to reset during gameplay or debugging
            loggedPawns.Clear();
        }

        public static float CalculateMoodProxy(Pawn pawn)
        {
            // Fail-safe check: Ensure pawn and its needs are valid; otherwise, return emergency value
            if (pawn == null || pawn.needs == null)
            {
                Log.Error("Pawn or its needs are null in CalculateMoodProxy.");
                return EmergencyFallbackValue;
            }

            float moodProxy = 0.0f;

            // Temperature-based calculation for MoodProxy
            float ambientTemperature = pawn.AmbientTemperature;

            if (ambientTemperature < SevereColdTemp)
            {
                moodProxy = -5f; // Severe cold
            }
            else if (ambientTemperature >= SevereColdTemp && ambientTemperature < ComfortMinTemp)
            {
                moodProxy = Mathf.Lerp(-5f, 3f, (ambientTemperature - SevereColdTemp) / (ComfortMinTemp - SevereColdTemp));
            }
            else if (ambientTemperature >= ComfortMinTemp && ambientTemperature < IdealMinTemp)
            {
                moodProxy = Mathf.Lerp(3f, 6f, (ambientTemperature - ComfortMinTemp) / (IdealMinTemp - ComfortMinTemp));
            }
            else if (ambientTemperature >= IdealMinTemp && ambientTemperature <= IdealMaxTemp)
            {
                moodProxy = 12f; // Ideal temperature range
            }
            else if (ambientTemperature > IdealMaxTemp && ambientTemperature <= ComfortMaxTemp)
            {
                moodProxy = Mathf.Lerp(12f, 0f, (ambientTemperature - IdealMaxTemp) / (ComfortMaxTemp - IdealMaxTemp));
            }
            else if (ambientTemperature > ComfortMaxTemp && ambientTemperature < SevereHeatTemp)
            {
                moodProxy = Mathf.Lerp(0f, -5f, (ambientTemperature - ComfortMaxTemp) / (SevereHeatTemp - ComfortMaxTemp));
            }
            else
            {
                moodProxy = -5f; // Severe heat
            }

            // Environmental adjustments
            if (pawn.Map.weatherManager.RainRate > 0.1f)
            {
                moodProxy += 3f;
            }

            if (!pawn.Position.Roofed(pawn.Map))
            {
                moodProxy += 3f;
            }

            // Adjustments based on hunger and rest
            if (pawn.needs.food.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            if (pawn.needs.rest.CurLevelPercentage < 0.2f)
            {
                moodProxy *= 0.5f;
            }

            // Clamp the final moodProxy value to ensure it stays within a reasonable range
            moodProxy = Mathf.Clamp(moodProxy, -6f, 21f);

            // If the calculation failed to produce a sane value, return the emergency fallback
            if (float.IsNaN(moodProxy) || moodProxy == 0f)
            {
                Log.Error($"Unexpected moodProxy value calculated for {pawn.LabelShort}: {moodProxy}. Returning emergency value.");
                return EmergencyFallbackValue;
            }

       /*     if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"Calculated MoodProxy for {pawn.LabelShort}: {moodProxy}");
            }*/

            return moodProxy;
        }

        // Static method to retrieve HediffComp_AnimalEmitter from a pawn
        public static HediffComp_AnimalEmitter GetAnimalEmitter(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                Log.Warning($"Cannot retrieve HediffComp_AnimalEmitter: Pawn {pawn?.LabelShort ?? "Unknown"} has no health or hediffSet.");
                return null;
            }

            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is HediffWithComps hediffWithComps)
                {
                    var comp = hediffWithComps.TryGetComp<HediffComp_AnimalEmitter>();
                    if (comp != null)
                    {
                        return comp;
                    }
                }
            }

            if (SymbiophoreMod.settings.EnableLogging)  // Ensuring logging is conditional
            {
                Log.Warning($"No HediffComp_AnimalEmitter found for pawn {pawn.LabelShort}");
            }
            return null;
        }
    }
}