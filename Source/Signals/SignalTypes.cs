namespace DIL_Symbiophore.Signals
{
    /// <summary>Player-legible creature health for emission.</summary>
    public enum HealthState : byte
    {
        Healthy = 0,
        Injured = 1,        // hard BaseMood = -3 (sickness, wounds, malaise)
        Catastrophic = 2    // BaseMood = -80 when gate fires
    }

    public enum DistanceBand : byte
    {
        Immediate = 0, // 0..7  1:1 envelope
        Near = 1,      // 8..29 global max, mapped steps
        Far = 2,       // 30..60 awareness → toward 1
        Beyond = 3     // >60 stop sustain
    }

    public enum MotionClass : sbyte
    {
        Approaching = -1,
        Steady = 0,
        Receding = 1
    }

    /// <summary>
    /// Humidity tiers from Witness (or stub 0.70 → Acceptable when Witness is absent).
    /// Default stub maps to Acceptable and does not constrain positive maxima.
    /// </summary>
    public enum HumidityTier : byte
    {
        Dangerous = 0,
        Discomfort = 1,
        Acceptable = 2,
        Ideal = 3
    }

    /// <summary>Creature-global envelope; no receiver, no PsychicSensitivity.</summary>
    public readonly struct EnvelopeSignal
    {
        public readonly HealthState Health;
        public readonly bool Raining;
        public readonly HumidityTier Humidity;
        public readonly int BaseMood; // 1..12 | 30 | -3 | -80 | 0

        public EnvelopeSignal(HealthState health, bool raining, HumidityTier humidity, int baseMood)
        {
            Health = health;
            Raining = raining;
            Humidity = humidity;
            BaseMood = baseMood;
        }

        /// <summary>Comfort need / silk scale — never rain 30 or catastrophe −80.</summary>
        public float ToComfort01()
        {
            if (BaseMood <= 0)
            {
                return 0f;
            }

            if (BaseMood > 12)
            {
                return 1f;
            }

            return BaseMood / 12f;
        }
    }

    /// <summary>Per-receiver aggregated spring state.</summary>
    public struct LinkSpring
    {
        public int LastDist;
        public int Displayed;
        public int Target;
        public DistanceBand Band;
        public MotionClass Motion;
        public int LastSampleTick;
        public int LastEmitterId;
    }

    /// <summary>Result of ring aggregation for one receiver.</summary>
    public readonly struct AggregatedTarget
    {
        public readonly DistanceBand Band;
        public readonly int EnvelopeMood;
        public readonly int Dist;
        public readonly int EmitterId;
        public readonly bool HasSignal;

        public AggregatedTarget(DistanceBand band, int envelopeMood, int dist, int emitterId, bool hasSignal)
        {
            Band = band;
            EnvelopeMood = envelopeMood;
            Dist = dist;
            EmitterId = emitterId;
            HasSignal = hasSignal;
        }

        public static AggregatedTarget None => new AggregatedTarget(DistanceBand.Beyond, 0, 999, 0, false);
    }
}
