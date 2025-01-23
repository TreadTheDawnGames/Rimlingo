using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimguistics
{
    public class RimguisticsSettings : ModSettings
    {
       public bool showFullLangKnowledge = false;
        public bool useDebugValues = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref showFullLangKnowledge, "showFullLangKnowledge", true);
            Scribe_Values.Look(ref useDebugValues, "useDebugValues", true);
            //Look(ref showFullLangKnowledge, "showFullLangKnowledge");

        }
        public void ResetToDefaults()
        {
            showFullLangKnowledge = false;
            useDebugValues = false;
        }
    }

    public class RimguisticsMod : Mod
    {
        public static RimguisticsSettings Settings;
        public static float cursorThickness = 2f; // Default thickness
        private Vector2 scrollPosition = Vector2.zero;

        public RimguisticsMod(ModContentPack content) : base(content)
        {
            
            Settings = GetSettings<RimguisticsSettings>();
            // Harmony patch
            var harmony = new HarmonyLib.Harmony("TreadTheDawnGames.Rimguistics");
            harmony.PatchAll();
            Log.Message("[Rimguistics] Harmony patches loaded successfully.");
        }

        public override string SettingsCategory()
        {
            return "Rimguistics";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float height = 1000f;
            Rect viewRect = new Rect(0f, 40f, inRect.width - 16f, height);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);


            if (listingStandard.ButtonText("Reset to Default"))
            {
                Settings.ResetToDefaults();
            }
            listingStandard.Gap(20f);
            listingStandard.CheckboxLabeled("- Show Full Language Knowledge", ref Settings.showFullLangKnowledge);
            listingStandard.Gap(20f);
            listingStandard.CheckboxLabeled("- Use Debug Values", ref Settings.useDebugValues);

            listingStandard.End();
        }
    }
}
