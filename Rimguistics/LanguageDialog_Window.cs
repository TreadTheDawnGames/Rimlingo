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

namespace Rimguistics
{
    internal class LanguageDialog_Window : Window
    {
        bool chosen = false;
        public Pawn_LangComp CachedComp;

        public LanguageDialog_Window()
        {
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            doCloseX = true;
        }

        [TweakValue("LangWindow Height Additional")]
        static float heightAdditional = 100;
        [TweakValue("LangWindow Height PerLang")]
        static float heightPerLang = 45;
        [TweakValue("Additional Width", 0, 1440)]
        static float additionalWidth= 300;
        
        [TweakValue("LangWindow X Position", 0, 2560)]
        static float WindowXPos= 513;
        [TweakValue("LangWindow Y Position", 0, 1440)]
        static float WindowYPos= 650;
        
        protected override void SetInitialSizeAndPosition()
        {

            string longestName = CachedComp.Languages?.MaxBy(l => l.Key.Length).Key ?? "";

            float height = (heightPerLang * CachedComp.MaxLangs) + heightAdditional;
            Rect viewRect = new Rect(WindowXPos, UI.screenHeight - WindowYPos, Text.CalcSize(longestName).x + additionalWidth, height);

            windowRect = viewRect;
            windowRect = windowRect.Rounded();
        }

        public override void DoWindowContents(Rect inRect)
        {

            if (!Find.Selector.SelectedPawns.Any())
            {
                Close(); 
                return;
            }
            var comp = LangUtils.GetPawnLangComp(Find.Selector.SelectedPawns.First());

            if (comp != CachedComp)
            {
                CachedComp = comp;
                SetInitialSizeAndPosition();
            }


            float height = (200 * CachedComp.MaxLangs) + 10;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(viewRect);
            
            listingStandard.Label(CachedComp.parent.Label);
//            optionalTitle = CachedComp.parent.Label;
            
            listingStandard.Gap();


            if(CachedComp != null && CachedComp.Languages.Any())
            {

                PawnBioUI.DrawLanguageSkills(listingStandard, CachedComp);
            }


            listingStandard.End();
        }
    }
}
