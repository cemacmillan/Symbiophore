using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public static class NeedsCardUtility
    {
        private static List<Need> customNeeds = new List<Need>();
        private const float NeedHeight = 50f;
        private const float NeedSpacing = 10f;

        public static Vector2 GetSize(Pawn pawn)
        {
            UpdateDisplayNeeds(pawn);
            return new Vector2(300f, Mathf.Max(400f, (customNeeds.Count * NeedHeight) + (customNeeds.Count * NeedSpacing)));
        }

        public static void DoNeeds(Rect rect, Pawn pawn, ref Vector2 scrollPos)
        {
            UpdateDisplayNeeds(pawn);

            float viewHeight = customNeeds.Count * (NeedHeight + NeedSpacing);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
            Widgets.BeginScrollView(rect, ref scrollPos, viewRect);

            try
            {
                float curY = 0f;
                foreach (var need in customNeeds)
                {
                    if (need is SymbiophoreCustomNeed customNeed)
                    {
                        customNeed.UpdateValue(); // Update value if it's a SymbiophoreCustomNeed

                        Rect needRect = new Rect(0f, curY, viewRect.width, NeedHeight);
                        need.DrawOnGUI(needRect, maxThresholdMarkers: 10, customMargin: -1f, drawArrows: true, doTooltip: true, rectForTooltip: needRect, drawLabel: true);
                        curY += NeedHeight + NeedSpacing;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"Exception in DoNeeds: {ex}");
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private static void UpdateDisplayNeeds(Pawn pawn)
        {
            customNeeds.Clear();
            customNeeds.AddRange(pawn.needs.AllNeeds.OfType<SymbiophoreCustomNeed>());

            // Update custom needs from the harmonizer and other sources
            foreach (var need in customNeeds.OfType<SymbiophoreCustomNeed>())
            {
                need.UpdateValue();
            }
        }
    }
}