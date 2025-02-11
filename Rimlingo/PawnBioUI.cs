using UnityEngine;
using Verse;

namespace Rimlingo
{
    public static class PawnBioUI
    {
        public static void DrawLanguageSkills(Rect rect, Pawn pawn)
        {
            var comp = LanguageUtility.GetLanguagesComp(pawn);
            if (comp == null)
            {
                Log.Warning($"[Rimlingo] No language component found for {pawn.LabelShort}.");
                return;
            }

            // Title
            Widgets.Label(rect, "Languages:");
            rect.y += Text.LineHeight + 2f;

            // Draw each language skill
            foreach (var kvp in comp.languageSkills)
            {
                string info = $"{kvp.Key}: {kvp.Value:F1}";
                Widgets.Label(rect, info);
                rect.y += Text.LineHeight;
            }
        }
    }
}
