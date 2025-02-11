using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Rimlingo
{
    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    public static class PawnBioPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Rect rect, Pawn pawn)
        {
            // Ensure the pawn has at least "Common" language if no other language is set
            var comp = LanguageUtility.GetLanguagesComp(pawn);
            if (comp != null && comp.languageSkills.Count == 0)
            {
                comp.SetLanguageSkill("Common", 1f); // Default to "Common" with a skill level of 1
            }

            // The vanilla method draws the top portion (name, faction, etc.)
            // then draws the childhood/adulthood around ~ 100-200 px down depending on scrolled height.

            // We'll guess we can place languages around y=240. 
            // If you have heavily modded UI, adjust this offset as needed.
            float languagePanelY = 160f;
            float languagePanelHeight = 60f;

            // Let's define a rect that is half the width, just as an example.
            var languageRect = new Rect(
                x: rect.x,
                y: rect.y + languagePanelY,
                width: rect.width * 0.48f,
                height: languagePanelHeight
            );

            PawnBioUI.DrawLanguageSkills(languageRect, pawn);

            // If you want to shift the rest of the vanilla UI down (so traits are below languages),
            // you'd do something more advanced: e.g., adjusting the "curY" reflection in the method.
            // But a minimal approach is just to paint over the standard location (which might overlap).
            // If we want to forcibly shift traits downward, we need a transpiler or reflection edits.
        }
    }
}