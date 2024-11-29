using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

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
        private const int TicksPerDay = 60000;

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
        private void InitializeEmitter(Pawn pawn)
        {
            string pawnName = pawn.LabelShort ?? "Unnamed Pawn";

            Hediff hediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(
                    DefDatabase<HediffDef>.GetNamed("SymbiophorePsychicEmanation"));
            if (hediff != null)
            {
                this.emitter = hediff.TryGetComp<HediffComp_AnimalEmitter>();
                if (this.emitter != null)
                {
                    /*  if (SymbiophoreMod.settings.EnableLogging)
                      {
                          Log.Message($"HediffComp_AnimalEmitter found and assigned for {pawnName}.");
                      }*/
                }
                else
                {
                    Log.Error($"Failed to assign HediffComp_AnimalEmitter for {pawnName}.");
                }
            }
            else if (SymbiophoreMod.settings.EnableLogging)
            {
                Log.Error($"Hediff SymbiophorePsychicEmanation not found on {pawnName}. Cannot initialize emitter.");
            }
        }

        // Initialize the skeinable properties
        private void InitializeSkeinableProps(Pawn pawn)
        {
            CompProperties_Skeinable skeinable = pawn.def.GetCompProperties<CompProperties_Skeinable>();
            if (skeinable != null)
            {
                skeinableProps = skeinable;
                /*if (SymbiophoreMod.settings.EnableLogging)
                {
                    string pawnName = pawn.LabelShort ?? "Unnamed Pawn";
                    Log.Message($"CompProperties_Skeinable found and assigned for {pawnName}. Resource Amount: {skeinableProps.resourceAmount}, Gather Interval Days: {skeinableProps.gatherResourcesIntervalDays}");
                }*/
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

        // Property to get and set the current level of the need
        public override float CurLevel
        {
            get => value;
            set => this.value = value; // Ensure the value is stored in the CurLevel
        }

        public override int GUIChangeArrow => 0; // No change arrow in GUI
        public override bool ShowOnNeedList => true; // Show on the need list

        // Override with functionality to control update frequency
        public override void NeedInterval()
        {
            updateCounter++;
            if (updateCounter >= updateFrequency)
            {
                updateCounter = 0; // Reset the counter
                UpdateValue(); // Update the value only every 4th time
            }
        }

        // Tooltip string to display the need's current level
        public override string GetTipString()
        {
            string tip = def.label + ": " + (CurLevel * 100f).ToString("F0") + "%";
            // Removed logging here to prevent excessive log entries
            return tip;
        }

        // Method to update the need value based on the MoodProxy from emitter
        public void UpdateValue()
        {
            string pawnName = pawn.LabelShort ?? "Unnamed Pawn";

            if (emitter != null)
            {
                if (def.defName == "SymbiophoreMood")
                {
                    /*  if (SymbiophoreMod.settings.EnableLogging)
                      {
                          Log.Message($"Updating value for SymbiophoreMood for {pawnName} with emitter. Current MoodProxy: {emitter.MoodProxy}");
                      }
       */
                    CurLevel = emitter.MoodProxy / 12f;

                    /*   if (SymbiophoreMod.settings.EnableLogging)
                       {
                           Log.Message($"Updated SymbiophoreMood value for {pawnName}: {CurLevel}");
                       }*/
                }
                else if (def.defName == "SymbiophoreFullness")
                {
                    var skeinableComp = pawn.TryGetComp<CompSkeinable>();
                    if (skeinableComp != null)
                    {
                        // Use the public property SkeinProgress to update fullness
                        CurLevel = skeinableComp.SkeinProgress;

                        /*  if (SymbiophoreMod.settings.EnableLogging)
                          {
                              Log.Message($"Updating value for SymbiophoreFullness for {pawnName} using skeinProgress. New Value: {CurLevel}");
                          }*/
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

        // Ensure data persistence by overriding ExposeData
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "curLevel", 0f);
        }
    }
}