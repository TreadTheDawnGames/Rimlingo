using UnityEngine;
using Verse;
using System;

namespace Rimguistics
{
    public static class PawnBioUI
    {
        /// <summary>
        /// Draws the language skills for <paramref name="pawn"/> in <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="pawn"></param>
        public static void DrawLanguageSkills(Rect rect, Pawn pawn)
        {
            var comp = LangUtils.GetPawnLangComp(pawn);
            if (comp == null)
            {
                Log.Warning($"[Rimguistics] No language component found for {pawn.LabelShort}.");
                return;
            }

            // Title
            Widgets.Label(rect, "Languages:");
            rect.y += Text.LineHeight + 2f;

            // Draw each language skill
            foreach (var kvp in comp.Languages)
            {
                string info = $"{kvp.Key}: {Math.Min(kvp.Value.Skill, 100f):F1}";
                if(RimguisticsMod.Settings.showFullLangKnowledge)
                {
                    info = $"{kvp.Key}: {kvp.Value.Skill:F1}";

                }
                Widgets.Label(rect, info);
                rect.y += Text.LineHeight;
            }
        }
    }
}
