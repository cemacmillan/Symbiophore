using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore.Signals
{
    /// <summary>
    /// Receiver-centric psychic field: aggregate max per ring, one spring + one shared thought per pawn.
    /// </summary>
    public class MapComponent_SymbiophoreSignals : MapComponent
    {
        private const int SampleInterval = 250;
        private readonly Dictionary<int, LinkSpring> springs = new Dictionary<int, LinkSpring>();

        private static readonly List<HediffComp_AnimalEmitter> EmitterBuffer = new List<HediffComp_AnimalEmitter>();
        private static readonly List<Pawn> ReceiverBuffer = new List<Pawn>();

        public MapComponent_SymbiophoreSignals(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            if (!map.IsHashIntervalTick(SampleInterval))
            {
                return;
            }

            CollectEmitters();
            if (EmitterBuffer.Count == 0)
            {
                DecayAllReceivers();
                return;
            }

            CollectReceivers();
            for (int i = 0; i < ReceiverBuffer.Count; i++)
            {
                UpdateReceiver(ReceiverBuffer[i]);
            }
        }

        private void CollectEmitters()
        {
            EmitterBuffer.Clear();
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p == null || p.Dead || !p.Spawned)
                {
                    continue;
                }

                HediffComp_AnimalEmitter emitter = SymbiophoreUtility.GetAnimalEmitter(p);
                if (emitter != null && SymbiophoreUtility.CanEmit(p))
                {
                    EmitterBuffer.Add(emitter);
                }
            }
        }

        private void CollectReceivers()
        {
            ReceiverBuffer.Clear();
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p != null && p.Spawned && !p.Dead && p.RaceProps.Humanlike && p.needs?.mood != null)
                {
                    ReceiverBuffer.Add(p);
                }
            }
        }

        private void DecayAllReceivers()
        {
            // No emitters: stop refreshing; let vanilla duration age thoughts out.
            springs.Clear();
        }

        private void UpdateReceiver(Pawn receiver)
        {
            AggregatedTarget agg = Aggregate(receiver);
            int id = receiver.thingIDNumber;

            if (!springs.TryGetValue(id, out LinkSpring spring))
            {
                spring = new LinkSpring
                {
                    LastDist = agg.HasSignal ? agg.Dist : SignalResolver.FarMax + 1,
                    Displayed = 0,
                    Target = 0,
                    Band = DistanceBand.Beyond,
                    Motion = MotionClass.Steady,
                    LastSampleTick = Find.TickManager.TicksGame,
                    LastEmitterId = 0
                };
            }

            MotionClass motion = agg.HasSignal
                ? SignalResolver.MotionOf(spring.LastDist, agg.Dist)
                : MotionClass.Receding;

            int target = agg.HasSignal
                ? SignalResolver.TargetFor(agg.EnvelopeMood, agg.Band, agg.Dist, motion)
                : 0;

            // Mode jumps (Injured/Catastrophic/Rain) snap; otherwise spring.
            bool modeJump = IsModeJump(spring.Displayed, target);
            int displayed = modeJump ? target : SignalResolver.StepSpring(spring.Displayed, target, motion);

            spring.LastDist = agg.HasSignal ? agg.Dist : spring.LastDist + 1;
            spring.Target = target;
            spring.Displayed = displayed;
            spring.Band = agg.Band;
            spring.Motion = motion;
            spring.LastSampleTick = Find.TickManager.TicksGame;
            spring.LastEmitterId = agg.EmitterId;
            springs[id] = spring;

            ApplyThought(receiver, displayed);
        }

        private static bool IsModeJump(int from, int to)
        {
            if (from == to)
            {
                return false;
            }

            // Rain / catastrophe / injured ↔ comfort: snap
            if (from == 30 || to == 30 || from == -80 || to == -80 || from == -3 || to == -3)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Immediate (0–7) max wins; else Near (8–29) global max; else Far (30–60) best awareness candidate.
        /// </summary>
        private AggregatedTarget Aggregate(Pawn receiver)
        {
            bool anyImm = false, anyNear = false, anyFar = false;
            int bestImmMood = 0, bestNearMood = 0, bestFarMood = 0;
            int bestImmDist = int.MaxValue, bestNearDist = int.MaxValue, bestFarDist = int.MaxValue;
            int bestImmId = 0, bestNearId = 0, bestFarId = 0;

            IntVec3 recvPos = receiver.Position;

            for (int i = 0; i < EmitterBuffer.Count; i++)
            {
                HediffComp_AnimalEmitter emitter = EmitterBuffer[i];
                Pawn ePawn = emitter.parent?.pawn;
                if (ePawn == null || ePawn == receiver || !ePawn.Spawned || ePawn.Map != map)
                {
                    continue;
                }

                int dist = Mathf.RoundToInt(recvPos.DistanceTo(ePawn.Position));
                DistanceBand band = SignalResolver.BandOf(dist);
                if (band == DistanceBand.Beyond)
                {
                    continue;
                }

                int mood = emitter.BaseMood;
                int eid = ePawn.thingIDNumber;

                switch (band)
                {
                    case DistanceBand.Immediate:
                        if (!anyImm || IsStrongerSignal(mood, bestImmMood, dist, bestImmDist))
                        {
                            anyImm = true;
                            bestImmMood = mood;
                            bestImmDist = dist;
                            bestImmId = eid;
                        }
                        break;

                    case DistanceBand.Near:
                        // Global max in 8–29; catastrophe overrides; else signed max; tie-break nearer.
                        if (!anyNear || IsStrongerSignal(mood, bestNearMood, dist, bestNearDist))
                        {
                            anyNear = true;
                            bestNearMood = mood;
                            bestNearDist = dist;
                            bestNearId = eid;
                        }
                        break;

                    case DistanceBand.Far:
                        if (!anyFar || IsStrongerSignal(mood, bestFarMood, dist, bestFarDist))
                        {
                            anyFar = true;
                            bestFarMood = mood;
                            bestFarDist = dist;
                            bestFarId = eid;
                        }
                        break;
                }
            }

            if (anyImm)
            {
                return new AggregatedTarget(DistanceBand.Immediate, bestImmMood, bestImmDist, bestImmId, true);
            }

            if (anyNear)
            {
                return new AggregatedTarget(DistanceBand.Near, bestNearMood, bestNearDist, bestNearId, true);
            }

            if (anyFar)
            {
                return new AggregatedTarget(DistanceBand.Far, bestFarMood, bestFarDist, bestFarId, true);
            }

            return AggregatedTarget.None;
        }

        /// <summary>Catastrophic (−80) wins; else higher signed mood; tie-break nearer.</summary>
        private static bool IsStrongerSignal(int mood, int bestMood, int dist, int bestDist)
        {
            if (mood == -80 && bestMood != -80)
            {
                return true;
            }

            if (bestMood == -80 && mood != -80)
            {
                return false;
            }

            if (mood > bestMood)
            {
                return true;
            }

            if (mood == bestMood && dist < bestDist)
            {
                return true;
            }

            return false;
        }

        private void ApplyThought(Pawn receiver, int displayed)
        {
            ThoughtDef def = DefOfs.SymbiophoreMoodThought;
            if (def == null || receiver.needs?.mood?.thoughts?.memories == null)
            {
                return;
            }

            Thought_Memory existing = receiver.needs.mood.thoughts.memories.GetFirstMemoryOfDef(def);

            if (!SignalResolver.HasSignalMagnitude(displayed) || !SignalResolver.IsMoodStage(displayed))
            {
                // Stop sustaining; leave existing memory to vanilla duration expiry.
                return;
            }

            int stage = SignalResolver.StageIndexForMood(displayed);

            if (existing != null)
            {
                if (existing.CurStageIndex != stage)
                {
                    existing.SetForcedStage(stage);
                }

                // Refresh age so the same shared stage does not flicker on medic runs.
                existing.age = 0;
                return;
            }

            Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(def, stage);
            receiver.needs.mood.thoughts.memories.TryGainMemory(memory);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // Springs are ephemeral; rebuild from live geometry after load.
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                springs.Clear();
            }
        }
    }
}
