using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DIL_Symbiophore
{
    public class Thought_SymbiophoreEmitter : Thought_Memory
    {
       // private bool hasLoggedNullEmitter = false;
        private bool _isCalculatingMoodOffset = false;
        // private bool loggedRecently = false;
        private int logCooldownTicks = 0;
        private const int LogCooldownDuration = 250;
        private Pawn lastLoggedPawn = null;
        public HediffComp_AnimalEmitter emitter { get; private set; }

        // Setter for the emitter, which will be used to assign the emitter
        public void SetEmitter(HediffComp_AnimalEmitter emitterComp)
        {
            emitter = emitterComp;
        }

        // Helper method to retrieve the emitter from the pawn's hediffs
        private HediffComp_AnimalEmitter GetEmitter()
        {
            return SymbiophoreUtility.GetAnimalEmitter(pawn);
        }

        // Override the label to include the emitter pawn's name
        public override string LabelCap => base.CurStage.label.Formatted((NamedArgument)(emitter?.parent.pawn?.LabelShort ?? "Unknown Emitter")).CapitalizeFirst();

        // Helper method to check if two pawns are in the same player-controlled caravan
        private bool AreInSamePlayerControlledCaravan(Pawn pawn1, Pawn pawn2)
        {
            return pawn1.IsPlayerControlledCaravanMember() && pawn2.IsPlayerControlledCaravanMember() && 
                   pawn1.GetCaravan() == pawn2.GetCaravan();
        }

        public override bool ShouldDiscard
        {
            get
            {
                if (emitter == null)
                {
                    return true;
                }

                Pawn emitterPawn = emitter?.parent?.pawn;
                if (emitterPawn == null || emitterPawn.health.Dead || !emitterPawn.health.capacities.CanBeAwake)
                {
                    // Pawn is either dead or unconscious
                    return true;
                }

                if (!emitterPawn.Spawned && !base.pawn.Spawned && AreInSamePlayerControlledCaravan(emitterPawn, base.pawn))
                {
                    return false;
                }

                if (emitterPawn.Spawned && base.pawn.Spawned && emitterPawn.Map == base.pawn.Map)
                {
                    return emitterPawn.Position.DistanceTo(base.pawn.Position) > emitter.Props.range;
                }

                return true;
            }
        }
        
        // Save/load the reference to the emitter
        public override void ExposeData()
        {
            base.ExposeData();

            // Use the getter/setter to manage emitter during saving/loading
            var localEmitter = emitter;
            Scribe_References.Look(ref localEmitter, "emitter");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                SetEmitter(localEmitter);
            }
        }

        public override float MoodOffset()
        {
            if (_isCalculatingMoodOffset)
            {
                Log.Warning($"MoodOffset calculation was re-entered for {pawn.LabelShort}, which might indicate a problem.");
                return 0f;
            }

            _isCalculatingMoodOffset = true;

            try
            {
                if (CurStage == null)
                {
                    Log.Error($"CurStage is null while ShouldDiscard is false on {def.defName} for {pawn.LabelShort}");
                    return 0f;
                }

                if (ThoughtUtility.ThoughtNullified(pawn, def))
                {
                    return 0f;
                }

                // Ensure that emitter is not null before proceeding
                if (emitter == null)
                {
                    // disabled because this simply not a rare condition due to pawns going out of range of each other
                    //Log.Error($"Emitter is null for {pawn.LabelShort} in MoodOffset calculation.");
                    return 0f;
                }

                // Use the emitter’s MoodProxy directly
                float moodProxy = emitter.MoodProxy;
                float psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity, applyPostProcess: true);
                float result = moodProxy * psychicSensitivity;

                // Handle logging with cooldown and pawn change check
           /*     if (SymbiophoreMod.settings.EnableLogging)
                {
                    if (logCooldownTicks <= 0 || pawn != lastLoggedPawn)
                    {
                        Log.Message($"MoodOffset calculated for {pawn.LabelShort}. MoodProxy: {moodProxy}, Psychic Sensitivity: {psychicSensitivity}, Result: {result}");
                        lastLoggedPawn = pawn;
                        logCooldownTicks = LogCooldownDuration;
                    }
                    else
                    {
                        logCooldownTicks--;
                    }
                }
*/
                return result;
            }
            finally
            {
                _isCalculatingMoodOffset = false;
            }
        }

        // Prevent merging of thoughts, as each instance is unique to its emitter
        public override bool TryMergeWithExistingMemory(out bool showBubble)
        {
            showBubble = false;
            return false;
        }

        // Group thoughts only if they share the same emitter
        public override bool GroupsWith(Thought other)
        {
            if (other is Thought_SymbiophoreEmitter otherEmitterThought)
            {
                return otherEmitterThought.emitter == emitter && base.GroupsWith(other);
            }
            return false;
        }
    }
}