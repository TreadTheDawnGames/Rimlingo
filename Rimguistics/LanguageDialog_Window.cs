using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using LudeonTK;
using RimWorld.Planet;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace Rimguistics
{
    internal class LanguageDialog_Window : Window
    {
        bool chosen = false;
        public Pawn_LangComp CachedComp;

        [TweakValue("heightAdditional", 0f,200f)]
        static float heightAdditional = 110;

        [TweakValue("heightPerLang", 0f,75f)]
        static float heightPerLang = 34;
        [TweakValue("heightPerLangPlayer", 0f,75f)]
        static float heightPerLangPlayer = 42;
        
        [TweakValue("isFactionAdditional", 0f,400f)]
        static float isFactionAdditional = 250;
        
        [TweakValue("isNotFactionAdditional", 0f,150f)]
        static float isNotFactionAdditional = 70f;
        
        static float WindowXPos = 539;
        static float WindowYPos = 740;

        private static LanguageDialog_Window instance;

        public LanguageDialog_Window(Pawn_LangComp comp)
        {
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            doCloseX = true;
            instance = this;
            CachedComp = comp;
        }
        
        public static void CloseWindowInstance()
        {
            if (instance != null)
                instance.Close();
            else
                Log.Error("[Rimguistics] Unable to close Language Window: Instance was null.");
        }

        protected override void SetInitialSizeAndPosition()
        {
            Text.Font = GameFont.Medium;

            if (CachedComp == null)
            {
                Log.Warning("[Rimguistics] Unable to open LanguageDialog_Window: CachedComp is null.");
                Close();
                return;
            }

            string longestLangName = CachedComp.Languages?.Keys.MaxBy(l => l.Length) ?? "";
            string label = CachedComp.parent.Label;

            float langTextWidth = Text.CalcSize($"{longestLangName}: {Math.Min(CachedComp.Languages[longestLangName].Skill, 100f):F1}").x;
            float labelTextWidth = Text.CalcSize(CachedComp.parent.Label).x;
            float languagesCountWidth = Text.CalcSize($"Languages ({CachedComp.Languages.Count}/{CachedComp.MaxLangs}):").x;

            float textWidth = Math.Max(Math.Max(langTextWidth, labelTextWidth), languagesCountWidth);

            Log.Message($"{longestLangName}: {langTextWidth}, {label}: {labelTextWidth}, {$"Languages ({CachedComp.Languages.Count}/{CachedComp.MaxLangs}):"}: {languagesCountWidth}, text width: {textWidth}") ;

            float height = ((CachedComp.parent.Faction.IsPlayer ? heightPerLangPlayer : heightPerLang) * CachedComp.MaxLangs) + heightAdditional;
            Rect viewRect = new Rect(WindowXPos, UI.screenHeight - WindowYPos, textWidth + (CachedComp.parent.Faction.IsPlayer ? isFactionAdditional : isNotFactionAdditional), height);
            windowRect = viewRect.Rounded();

        }

        public override void DoWindowContents(Rect inRect)
        {
            //if there is no selected pawn, close the window
            if (!Find.Selector.SelectedPawns.Any())
            {
                Close(); 
                return;
            }

            //get info for the selected pawn
            var selectedPawn = Find.Selector.SelectedPawns.First();

            //if pawn is an animal, close.
            if (selectedPawn.AnimalOrWildMan() && !selectedPawn.IsWildMan())
            {
                Close();
                return;
            }
            var comp = LangUtils.GetPawnLangComp(selectedPawn);
            
            
            var tabs = selectedPawn.GetInspectTabs();

            //check to make sure the social tab is open, and if it's not, close the language window.
            foreach (var tab in tabs)
            {
                if (tab is ITab_Pawn_Social)
                {
                    if (!InspectPaneUtility.IsOpen(tab, (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow))
                    {
                        Close();
                        return;
                    }
                }
            }

            //if the inspect window isn't open, close the window.
            if (!MainButtonDefOf.Inspect.TabWindow.IsOpen)
            {
                Close();
                return;
            }

            //cache the comp
            if (comp != CachedComp)
            {
                CachedComp = comp;
                SetInitialSizeAndPosition();
            }

            //place the language window items
            float height = (200 * CachedComp.MaxLangs) + 10;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(viewRect);
            
            listingStandard.Label(CachedComp.parent.Label);
            
            listingStandard.Gap();

            if(CachedComp != null && CachedComp.Languages.Any())
            {
                PawnBioUI.DrawLanguageSkills(listingStandard, CachedComp);
            }

            listingStandard.End();
        }
    }
}
