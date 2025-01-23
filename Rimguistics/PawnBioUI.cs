using UnityEngine;
using Verse;
using System;

namespace Rimguistics
{
    public static class PawnBioUI
    {
        /// <summary>
        /// Draws the language skills for <paramref name="comp"/> in <paramref name="standard"/>.
        /// </summary>
        /// <param name="standard"></param>
        /// <param name="comp"></param>
        public static void DrawLanguageSkills(Listing_Standard standard, Pawn_LangComp comp)
        {
            if (comp == null)
            {
                Log.Warning($"[Rimguistics] No language component found to render language window.");
                return;
            }
            if (standard.ButtonTextLabeled($"Languages ({comp.Languages.Count}/{comp.MaxLangs}):", "Reset Preferred"))
            {
                comp.SetPreferredLanguage(null);
            }
            standard.Gap();
            // Draw each language skill
            foreach (var kvp in comp.Languages)
            {
                string info = $"{kvp.Key}: {Math.Min(kvp.Value.Skill, 100f):F1}";
                if (RimguisticsMod.Settings.showFullLangKnowledge)
                {
                    info = $"{kvp.Key}: {kvp.Value.Skill:F1}";
                }
                var knowledgeColor = kvp.Value.PreferredLanguage ? Color.green : GetColorForLearnState(kvp.Value.Skill);


                if (standard.ButtonTextLabeled(info.Colorize(knowledgeColor), "Set as Preferred"))
                {
                    comp.SetPreferredLanguage(kvp.Value.LangName);
                }
                //reset color
                GUI.color = Color.white;
                standard.Gap();
            }
        }

        static Color GetColorForLearnState(float langSkill)
        {
            float grayValue = Math.Min(langSkill, 100f);
            grayValue = Math.Max(0.2f, grayValue / 100f);

            Color c = new Color(grayValue, grayValue, grayValue);
            return c;
        }
    }
}
