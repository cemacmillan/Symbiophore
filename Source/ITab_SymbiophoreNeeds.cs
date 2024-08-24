using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class ITab_SymbiophoreNeeds : ITab
    {
        private Vector2 scrollPos;

        public ITab_SymbiophoreNeeds()
        {
            size = new Vector2(300f, 400f);
            labelKey = "TabSymbiophoreNeeds";
        }

        public override bool IsVisible
        {
            get
            {
                return SelPawn != null && SelPawn.def.defName == "Symbiophore";
            }
        }

        protected override void UpdateSize()
        {
            size = DIL_Symbiophore.NeedsCardUtility.GetSize(SelPawn);
        }

        public override void OnOpen()
        {
            scrollPos = default(Vector2);
        }

        protected override void FillTab()
        {
            try
            {
                Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
                DIL_Symbiophore.NeedsCardUtility.DoNeeds(rect, SelPawn, ref scrollPos);
            }
            catch (System.Exception ex)
            {
                Log.Error($"Exception in FillTab: {ex}");
            }
        }
    }
}