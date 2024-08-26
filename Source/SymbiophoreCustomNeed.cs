using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace DIL_Symbiophore
{
    public class SymbiophoreCustomNeed : Need
    {
        private float value;
        //what got messed up before here, is this somehow became static and shared between symbiophore instances. Hahahahahaha......
        private SymbiophorePsychicEmitter harmonizer;

        public SymbiophoreCustomNeed(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float> { 0.25f, 0.5f, 0.75f }; // Not actually used
        }

        public SymbiophoreCustomNeed(Pawn pawn, SymbiophorePsychicEmitter harmonizer) : this(pawn)
        {
            this.harmonizer = harmonizer;
        }

        public override float CurLevel
        {
            get => value;
            set => this.value = value;
        }

        public override int GUIChangeArrow => 0;
        public override bool ShowOnNeedList => true;

        public override void NeedInterval() { }

        public override string GetTipString()
        {
            return def.label + ": " + (value * 100f).ToString("F0") + "%";
        }

        public void UpdateValue()
        {
            if (harmonizer != null && def.defName == "SymbiophoreMood")
            {
                value = harmonizer.MoodProxy / 12f;
            }
        }
    }
}