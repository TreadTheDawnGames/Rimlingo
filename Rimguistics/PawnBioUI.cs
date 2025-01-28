using UnityEngine;
using Verse;
using System;
using System.Linq;

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
            if (comp.parent.Faction.IsPlayer)
            {
                if (standard.ButtonTextLabeled($"Languages ({comp.Languages.Count}/{comp.MaxLangs}):", "Reset Preferred"))
                {
                    comp.SetPreferredLanguage(null);
                }
            }
            else
            {
                standard.Label($"Languages ({comp.Languages.Count}/{comp.MaxLangs}):");
            }

            standard.Gap();

            // Draw each language skill
            int langCount = 0;
            foreach (var kvp in comp.Languages.OrderBy(kvp => kvp.Value.Skill).Reverse())
            {
                string info = $"{kvp.Key}: {Math.Min(kvp.Value.Skill, 100f):F1}";
                if (RimguisticsMod.Settings.showFullLangKnowledge)
                {
                    info = $"{kvp.Key}: {kvp.Value.Skill:F1}";
                }
                var knowledgeColor = kvp.Value.PreferredLanguage ? Color.green : GetColorForLearnState(kvp.Value.Skill);

                if (comp.parent.Faction == Find.FactionManager.OfPlayer)
                {
                    if (standard.ButtonTextLabeled(info.Colorize(knowledgeColor), "Set as Preferred"))
                    {
                        comp.SetPreferredLanguage(kvp.Value.LangName);
                    }
                }
                else
                {
                    standard.Label(info.Colorize(knowledgeColor));
                }
                //reset color
                GUI.color = Color.white;
                standard.Gap();
                langCount++;
            }

            while (langCount < comp.MaxLangs)
            {
                standard.Label("- - - - -".Colorize(new Color(0.2f, 0.2f, 0.2f)));
                standard.Gap();
                langCount++;
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
