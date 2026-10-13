using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore.Signals
{
    public static class SignalResolver
    {
        public const int ImmediateMax = 7;
        public const int NearMax = 29;
        public const int FarMax = 60;

        // Temperature envelope breakpoints (Celsius), matched to former MoodProxy bands.
        private const float SevereColdTemp = 7f;
        private const float ComfortMinTemp = 13f;
        private const float IdealMinTemp = 28f;
        private const float IdealMaxTemp = 37f;
        private const float ComfortMaxTemp = 49f;
        private const float SevereHeatTemp = 50f;

        /// <summary>
        /// Fallback when Witness is not installed. 0.70 → Acceptable; does not constrain positive maxima.
        /// </summary>
        public const float StubHumidity = 0.70f;

        public static HumidityTier TierFromHumidity(float humidity01)
        {
            if (humidity01 < 0.10f)
            {
                return HumidityTier.Dangerous;
            }

            if (humidity01 < 0.60f)
            {
                return HumidityTier.Discomfort;
            }

            if (humidity01 < 0.95f)
            {
                return HumidityTier.Acceptable;
            }

            return HumidityTier.Ideal;
        }

        /// <summary>
        /// Climate feel from Witness when present; otherwise <see cref="StubHumidity"/>.
        /// </summary>
        public static float CurrentHumidity(Pawn emitter)
        {
            if (WitnessHumidityReflection.TryGetHumidity(emitter, out float humidity))
            {
                return humidity;
            }

            return StubHumidity;
        }

        public static HealthState ResolveHealth(Pawn emitter)
        {
            if (emitter == null || emitter.Dead)
            {
                return HealthState.Catastrophic;
            }

            // Catastrophic gate (stub): on fire. Expand later (bleed-out alone, eaten, rage…).
            if (emitter.IsBurning())
            {
                return HealthState.Catastrophic;
            }

            float health = emitter.health?.summaryHealth?.SummaryHealthPercent ?? 1f;
            if (health < 0.25f)
            {
                return HealthState.Catastrophic;
            }

            // Injured includes sickness and all non-healthy states.
            if (health < 0.85f || HasSickOrInjuredHediffs(emitter))
            {
                return HealthState.Injured;
            }

            return HealthState.Healthy;
        }

        private static bool HasSickOrInjuredHediffs(Pawn pawn)
        {
            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null)
            {
                return false;
            }

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h == null || !h.Visible)
                {
                    continue;
                }

                if (h.def.isBad && h.def.makesSickThought)
                {
                    return true;
                }

                if (h is Hediff_Injury injury && !injury.IsPermanent() && injury.Severity > 0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        public static EnvelopeSignal ResolveEnvelope(Pawn emitter)
        {
            if (emitter == null || emitter.Dead || emitter.Map == null)
            {
                return new EnvelopeSignal(HealthState.Catastrophic, false, HumidityTier.Acceptable, 0);
            }

            HealthState health = ResolveHealth(emitter);
            bool raining = emitter.Map.weatherManager.RainRate > 0.1f;
            float humidity = CurrentHumidity(emitter);
            HumidityTier humidityTier = TierFromHumidity(humidity);

            if (health == HealthState.Catastrophic)
            {
                return new EnvelopeSignal(health, raining, humidityTier, -80);
            }

            if (health == HealthState.Injured)
            {
                return new EnvelopeSignal(health, raining, humidityTier, -3);
            }

            // Humidity Acceptable/Ideal under Witness (or stub 0.70): no positive constraint.
            // Discomfort/Dangerous + below ideal temp would later constrain or flip.
            int baseMood;
            if (raining)
            {
                baseMood = 30;
            }
            else
            {
                baseMood = ComfortFromTemperature(emitter.AmbientTemperature);
            }

            // High humidity + below temp range → reserved sign flip.
            if (humidityTier == HumidityTier.Ideal && emitter.AmbientTemperature < IdealMinTemp)
            {
                // Reserved path until Ideal is common under Witness rain / jungle biomes.
            }

            return new EnvelopeSignal(health, raining, humidityTier, baseMood);
        }

        /// <summary>Discrete Normal envelope from temperature (healthy, not raining).</summary>
        public static int ComfortFromTemperature(float ambientTemperature)
        {
            if (ambientTemperature >= IdealMinTemp && ambientTemperature <= IdealMaxTemp)
            {
                return 12;
            }

            if (ambientTemperature >= ComfortMinTemp && ambientTemperature < IdealMinTemp)
            {
                // Cool comfort → mid envelope
                return 7;
            }

            if (ambientTemperature > IdealMaxTemp && ambientTemperature <= ComfortMaxTemp)
            {
                return 7;
            }

            if (ambientTemperature >= SevereColdTemp && ambientTemperature < ComfortMinTemp)
            {
                return 3;
            }

            if (ambientTemperature > ComfortMaxTemp && ambientTemperature < SevereHeatTemp)
            {
                return 3;
            }

            // Severe cold/heat — machine barely operating
            return 1;
        }

        public static DistanceBand BandOf(int dist)
        {
            if (dist <= ImmediateMax)
            {
                return DistanceBand.Immediate;
            }

            if (dist <= NearMax)
            {
                return DistanceBand.Near;
            }

            if (dist <= FarMax)
            {
                return DistanceBand.Far;
            }

            return DistanceBand.Beyond;
        }

        public static MotionClass MotionOf(int prevDist, int dist)
        {
            int delta = dist - prevDist;
            if (delta <= -1)
            {
                return MotionClass.Approaching;
            }

            if (delta >= 1)
            {
                return MotionClass.Receding;
            }

            return MotionClass.Steady;
        }

        /// <summary>
        /// Map aggregated envelope + band + motion → integer target for the spring.
        /// Near uses global max envelope then distance-mapped steps (8..29).
        /// Far collapses toward awareness 3/2/1.
        /// </summary>
        public static int TargetFor(int envelopeMood, DistanceBand band, int dist, MotionClass motion)
        {
            if (!HasSignalMagnitude(envelopeMood) || band == DistanceBand.Beyond)
            {
                return 0;
            }

            // Negative envelopes pass through; Immediate 1:1, Near/Far keep sign at reduced awareness where needed.
            if (envelopeMood < 0)
            {
                if (band == DistanceBand.Immediate)
                {
                    return envelopeMood;
                }

                if (band == DistanceBand.Near)
                {
                    // Distressed linger across Near at full unhappy magnitude (same −3/−80 everywhere).
                    return envelopeMood;
                }

                // Far: weak awareness of distress — still same stage if −3; catastrophe still −80 if somehow Far.
                return envelopeMood;
            }

            switch (band)
            {
                case DistanceBand.Immediate:
                    return envelopeMood;

                case DistanceBand.Near:
                    return MapNearDistance(envelopeMood, dist);

                case DistanceBand.Far:
                    return FarAwareness(motion);

                default:
                    return 0;
            }
        }

        /// <summary>8..29 → discrete attenuation of positive envelope toward a few steps.</summary>
        public static int MapNearDistance(int envelopeMood, int dist)
        {
            if (envelopeMood <= 0)
            {
                return envelopeMood;
            }

            // At 8: full (or nearly); at 29: weak. Integer steps only.
            float t = Mathf.InverseLerp(ImmediateMax + 1, NearMax, dist); // 0 at 8 … 1 at 29
            int minNear = Math.Max(1, envelopeMood / 4);
            int mapped = Mathf.RoundToInt(Mathf.Lerp(envelopeMood, minNear, t));
            return Math.Max(1, mapped);
        }

        public static int FarAwareness(MotionClass motion)
        {
            switch (motion)
            {
                case MotionClass.Approaching:
                    return 3;
                case MotionClass.Receding:
                    return 1;
                default:
                    return 2;
            }
        }

        public static bool HasSignalMagnitude(int mood) => mood != 0;

        /// <summary>Guess-then-relax one integer step toward target. Receding slows ascent; approaching slows decay.</summary>
        public static int StepSpring(int displayed, int target, MotionClass motion)
        {
            if (displayed == target)
            {
                return displayed;
            }

            int step = displayed < target ? 1 : -1;

            // Motion biases: approaching prefers upward; receding prefers downward (time decays).
            if (motion == MotionClass.Receding && step > 0)
            {
                // Harder to climb while leaving — stay unless already equal
                return displayed;
            }

            if (motion == MotionClass.Approaching && step < 0)
            {
                // Salience holds; decay one extra-slow: allow one step only every other call handled by caller,
                // here just take the step toward target normally for simplicity on approach holds.
            }

            return displayed + step;
        }

        /// <summary>Thought stage index for a mood magnitude. Must match Thoughts_Misc.xml stage order.</summary>
        public static int StageIndexForMood(int mood)
        {
            switch (mood)
            {
                case -80: return 0;
                case -3: return 1;
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                case 6: return 7;
                case 7: return 8;
                case 8: return 9;
                case 9: return 10;
                case 10: return 11;
                case 11: return 12;
                case 12: return 13;
                case 30: return 14;
                default:
                    // Snap unknown positives into 1..12, negatives toward −3
                    if (mood <= -40) return 0;
                    if (mood < 0) return 1;
                    if (mood > 12 && mood < 30) return 13;
                    if (mood >= 30) return 14;
                    return Mathf.Clamp(mood, 1, 12) + 1; // 1→2 … 12→13
            }
        }

        public static bool IsMoodStage(int mood)
        {
            return mood == -80 || mood == -3 || mood == 30 || (mood >= 1 && mood <= 12);
        }
    }
}
