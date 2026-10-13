using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Xml;

namespace DIL_Symbiophore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SymbiophoreCustomNeed : Need
    {
        private float value;
        private HediffComp_AnimalEmitter emitter;
        private CompProperties_Skeinable skeinableProps;
        private int updateCounter = 0; // Counter to control update frequency
        private const int updateFrequency = 4; // Update only 1/4 as often
        // private const int TicksPerDay = 60000;

        // Constructor when only Pawn is provided
        public SymbiophoreCustomNeed(Pawn pawn) : base(pawn)
        {
            string pawnName = pawn.LabelShort ?? "Unnamed Pawn";
            if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Message($"SymbiophoreCustomNeed initialized for {pawnName}.");
            }

            this.threshPercents = new List<float> { 0.25f, 0.5f, 0.75f };

            // Try to find and assign the emitter hediff if it's available
            InitializeEmitter(pawn);
            InitializeSkeinableProps(pawn);
        }

        // Initialize the emitter based on the pawn's Hediff
        private void InitializeEmitter(Pawn emitterPawn)
        {
            string pawnName = emitterPawn.LabelShort ?? "Unnamed Pawn";

            Hediff hediff =
                emitterPawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation"));
            if (hediff != null)
            {
                emitter = hediff.TryGetComp<HediffComp_AnimalEmitter>();
                if (emitter == null)
                {
                    Log.Error($"Failed to assign HediffComp_AnimalEmitter for {pawnName}.");
                }
            }
            else if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Error($"Hediff SymbiophorePsychicEmanation not found on {pawnName}. Cannot initialize emitter.");
            }
        }
        
        private void InitializeSkeinableProps(Pawn pawn)
        {
            CompProperties_Skeinable skeinable = pawn.def.GetCompProperties<CompProperties_Skeinable>();
            if (skeinable != null)
            {
                skeinableProps = skeinable;
            }
            else
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    string pawnName = pawn.LabelShort ?? "Unnamed Pawn";
                    Log.Error(
                        $"CompProperties_Skeinable not found on {pawnName}. Cannot initialize skeinable properties.");
                }
            }
        }
        
        public override float CurLevel
        {
            get => value;
            set => this.value = value;
        }

        public override int GUIChangeArrow => 0; // No change arrow in GUI
        public override bool ShowOnNeedList => true; // Show on the need list
        
        public override void NeedInterval()
        {
            updateCounter++;
            if (updateCounter >= updateFrequency)
            {
                updateCounter = 0; // Reset the counter
                UpdateValue(); // Update the value only every 4th time
            }
        }
        
        public override string GetTipString()
        {
            string tip = def.label + ": " + (CurLevel * 100f).ToString("F0") + "%";
            return tip;
        }
        
        public void UpdateValue()
        {
            string pawnName = pawn.LabelShort ?? "Unnamed Pawn";

            if (emitter != null)
            {
                if (def.defName == "SymbiophoreMood")
                {
                    // Comfort scale only — envelope.ToComfort01 via MoodProxy property.
                    CurLevel = UnityEngine.Mathf.Clamp01(emitter.MoodProxy / 12f);
                }
                else if (def.defName == "SymbiophoreFullness")
                {
                    var skeinableComp = pawn.TryGetComp<CompSkeinable>();
                    if (skeinableComp != null)
                    {
                        CurLevel = skeinableComp.SkeinProgress;
                    }
                    else
                    {
                        if (SymbiophoreMod.settings.EnableLogging)
                        {
                            Log.Error(
                                $"CompSkeinable is null in UpdateValue for {pawnName}. Fullness cannot be updated.");
                        }
                    }
                }
                else
                {
                    if (SymbiophoreMod.settings.EnableLogging)
                    {
                        Log.Warning(
                            $"Unrecognized defName '{def.defName}' in UpdateValue for {pawnName}. No action taken.");
                    }
                }
            }
            else
            {
                if (SymbiophoreMod.settings.EnableLogging)
                {
                    Log.Error(
                        $"HediffComp_AnimalEmitter is null in UpdateValue for {pawnName}. This could cause issues with need updates.");
                }
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
    
            // append defName so there's no confusion with the game's curLevel
            string key = $"curLevel_{def.defName}";
            float oldValue = value;

            // Serialize the value with the unique key
            Scribe_Values.Look(ref value, key, 0f);

            if (SymbiophoreMod.settings.EnableLogging)
            {
                string pawnName = pawn?.LabelShort ?? "Unknown Pawn";
                Log.Message($"ExposeData for {pawnName} (ID: {pawn?.thingIDNumber}). Old Value: {oldValue}, New Value: {value}, Key: {key}");
            }
        }
        
        /*public override void ExposeData()
        {
            // Enter a unique node to avoid key collisions
            Scribe.EnterNode(this.def.defName); // Use defName for clarity in the save file

            try
            {
                // Call base to handle necessary base-level serialization
                base.ExposeData();

                // Scribe custom value
                float oldValue = value;
                Scribe_Values.Look(ref value, "curLevel", 0f);

                if (SymbiophoreMod.settings.EnableLogging)
                {
                    string pawnName = pawn?.LabelShort ?? "Unknown Pawn";
                    Log.Message($"ExposeData for {pawnName} (ID: {pawn?.thingIDNumber}). Old Value: {oldValue}, New Value: {value}");
                }
            }
            finally
            {
                // Always exit the node to maintain XML integrity
                Scribe.ExitNode();
            }
        }*/
        
        /*public override void ExposeData()
        { 
            base.ExposeData();
            float oldValue = value;
            Scribe_Values.Look(ref value, "curLevel", 0f);
            if (SymbiophoreMod.settings.EnableLogging)
            {
                string pawnName = pawn?.LabelShort ?? "Unknown Pawn";
                Log.Message($"ExposeData for {pawnName} (ID: {pawn?.thingIDNumber}). Old Value: {oldValue}, New Value: {value}");
            }
        }*/
    }
}