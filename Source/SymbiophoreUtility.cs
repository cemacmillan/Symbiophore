using RimWorld;
using Verse;
using System.Collections.Generic;
using DIL_Symbiophore.Signals;

namespace DIL_Symbiophore
{
    public static class SymbiophoreUtility
    {
        private static HashSet<Pawn> loggedPawns = new HashSet<Pawn>();

        public static bool CanAccumulate(Pawn pawn, bool logErrorOnce = false)
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

        // Increments on RareTick; math is unusual by design.
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

        public static void ClearLoggedPawns()
        {
            loggedPawns.Clear();
        }

        /// <summary>Discrete envelope; replaces continuous CalculateMoodProxy.</summary>
        public static EnvelopeSignal CalculateEnvelope(Pawn pawn)
        {
            return SignalResolver.ResolveEnvelope(pawn);
        }

        /// <summary>
        /// Legacy surface — routes through discrete envelope. Mood 0 is valid.
        /// </summary>
        public static float CalculateMoodProxy(Pawn pawn)
        {
            return SignalResolver.ResolveEnvelope(pawn).BaseMood;
        }

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

            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Warning($"No HediffComp_AnimalEmitter found for pawn {pawn.LabelShort}");
            }
            return null;
        }
    }
}
