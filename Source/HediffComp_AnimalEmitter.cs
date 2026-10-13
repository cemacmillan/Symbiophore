using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using DIL_Symbiophore.Signals;

namespace DIL_Symbiophore
{
    public class HediffComp_AnimalEmitter : HediffComp, ILoadReferenceable
    {
        public HediffCompProperties_AnimalEmitter Props => (HediffCompProperties_AnimalEmitter)props;

        private EnvelopeSignal envelope;
        private int baseMood = 7;

        /// <summary>Discrete envelope mood (creature-global). Receiver aggregation reads this.</summary>
        public int BaseMood => baseMood;

        public EnvelopeSignal Envelope => envelope;

        /// <summary>Legacy float surface for Need/Silk; comfort-normalized and never rain/catastrophe inflated.</summary>
        public float MoodProxy
        {
            get => envelope.ToComfort01() * 12f;
            set
            {
                // Reads only; legacy sets ignored in favor of ResolveEnvelope.
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message($"MoodProxy set ignored ({value}) for {parent?.pawn?.LabelShort ?? "Unknown"}; envelope-driven.");
                }
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            Pawn pawn = parent?.pawn;
            if (pawn != null && pawn.Map != null)
            {
                envelope = SignalResolver.ResolveEnvelope(pawn);
                baseMood = envelope.BaseMood;
            }
            else
            {
                envelope = new EnvelopeSignal(HealthState.Healthy, false, HumidityTier.Acceptable, 7);
                baseMood = 7;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref baseMood, "BaseMood", 7);
            // Rebuild envelope from baseMood on load; full resolve on next tick.
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                envelope = new EnvelopeSignal(HealthState.Healthy, false, HumidityTier.Acceptable, baseMood);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = parent?.pawn;
            if (pawn == null || pawn.Dead || pawn.health == null || pawn.Map == null)
            {
                return;
            }

            // Envelope only — receivers are handled by MapComponent_SymbiophoreSignals.
            if (pawn.IsHashIntervalTick(250))
            {
                envelope = SignalResolver.ResolveEnvelope(pawn);
                baseMood = envelope.BaseMood;

                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Message(
                        $"[Symbiophore] Envelope {pawn.LabelShort}: mood={baseMood} health={envelope.Health} rain={envelope.Raining} humidity={envelope.Humidity}");
                }
            }
        }

        public string GetUniqueLoadID()
        {
            return $"HediffComp_AnimalEmitter_{parent?.GetUniqueLoadID() ?? "Unknown"}";
        }
    }

    public class HediffCompProperties_AnimalEmitter : HediffCompProperties
    {
        // Awareness horizon; MapComponent bands go to 60. Kept for caravan / legacy readers.
        public float range = 60f;
        public ThoughtDef thought;
        public float moodFactor = 1f;

        public HediffCompProperties_AnimalEmitter()
        {
            this.compClass = typeof(HediffComp_AnimalEmitter);
        }
    }
}
