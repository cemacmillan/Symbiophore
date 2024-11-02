using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Linq;

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
        // This only applies to mood effects - skeining (getting silk resource) won't happen on caravans
        // at present.
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
        
        public override void ExposeData()
        {
            base.ExposeData();

            // Save/load the unique ID of the emitter's parent pawn
            string emitterPawnID = emitter?.parent?.pawn?.GetUniqueLoadID();
            Scribe_Values.Look(ref emitterPawnID, "emitterPawnID");

            // Save/load the unique ID of the HediffWithComps that contains the HediffComp_AnimalEmitter
            string emitterHediffID = emitter?.parent?.GetUniqueLoadID();
            Scribe_Values.Look(ref emitterHediffID, "emitterHediffID");

            if (Scribe.mode == LoadSaveMode.LoadingVars && emitterPawnID != null && emitterHediffID != null)
            {
                // Resolve the emitter using the saved IDs
                Pawn emitterPawn = Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(p => p.GetUniqueLoadID() == emitterPawnID);
                if (emitterPawn != null)
                {
                    // Find the HediffWithComps using the unique ID and then get the HediffComp_AnimalEmitter
                    var hediff = emitterPawn.health.hediffSet.hediffs
                        .OfType<HediffWithComps>()
                        .FirstOrDefault(h => h.GetUniqueLoadID() == emitterHediffID);
            
                    if (hediff != null)
                    {
                        var emitterComp = hediff.TryGetComp<HediffComp_AnimalEmitter>();
                        SetEmitter(emitterComp);
                    }
                }
                else
                {
                    Log.Warning($"Failed to find emitter pawn with ID {emitterPawnID} for Thought_SymbiophoreEmitter.");
                }
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
                    // Not an error but a normal condition due to pawns going out of range of each other
                    //Log.Error($"Emitter is null for {pawn.LabelShort} in MoodOffset calculation.");
                    return 0f;
                }

                // Use the emitter’s MoodProxy directly
                float moodProxy = emitter.MoodProxy;
                float psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity, applyPostProcess: true);
                float result = moodProxy * psychicSensitivity;
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