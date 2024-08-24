using UnityEngine;
using Verse;

namespace DIL_Symbiophore
{
    public class SymbiophoreModSettingsWindow : Window
    {
        private Vector2 scrollPosition;
        public SymbiophoreModSettings modSettings;

        public SymbiophoreModSettingsWindow(SymbiophoreModSettings settings)
        {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            modSettings = settings;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 30f);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, contentRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            listing.CheckboxLabeled("Enable Logging", ref modSettings.EnableLogging);

            // Add the new checkboxes
            listing.CheckboxLabeled("Symbiophores Reproduce [not implemented]", ref modSettings.SymbiophoreReproduction);
            listing.CheckboxLabeled("Force Mood Effect to 9 [not implemented]", ref modSettings.ForceMoodEffect);

            listing.End();
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 25f, inRect.width, 25f), "Save"))
            {
                modSettings.Write();
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            windowRect = new Rect(0f, 0f, 400f, 300f);
            windowRect.center = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            modSettings.Write();
        }
    }
}