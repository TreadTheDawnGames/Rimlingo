using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimlingo
{
    public static class LanguageUtility
    {
        public static CompPawnLanguages GetLanguagesComp(Pawn pawn)
        {
            return pawn?.TryGetComp<CompPawnLanguages>();
        }

        public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            return GetLanguagesComp(pawn)?.GetLanguageSkill(langDefName) ?? 0f;
        }

        public static void IncreaseLanguageSkill(Pawn pawn, string langDefName, float amount)
        {
            var comp = GetLanguagesComp(pawn);
            if (comp == null) return;

            float current = comp.GetLanguageSkill(langDefName);
            float newVal = Math.Min(current + amount, 100f);
            comp.SetLanguageSkill(langDefName, newVal);

            Log.Message($"[Rimlingo] {pawn.LabelShort} gained {amount:F1} in {langDefName}, now {newVal:F1}.");
        }

        public static int GetPawnIntelligence(Pawn pawn)
        {
            var skill = pawn?.skills?.GetSkill(SkillDefOf.Intellectual);
            return skill?.Level ?? 0;
        }

        public static string DetermineBestLanguage(Pawn initiator, Pawn recipient)
        {
            var compA = GetLanguagesComp(initiator);
            var compB = GetLanguagesComp(recipient);
            if (compA == null || compB == null) return null;

            var languagesA = compA.languageSkills.Where(kv => kv.Value > 1f).Select(kv => kv.Key).ToList();
            var languagesB = compB.languageSkills.Where(kv => kv.Value > 1f).Select(kv => kv.Key).ToList();
            var mutual = languagesA.Intersect(languagesB).ToList();
            if (!mutual.Any()) return null;

            var nonCommon = mutual.FirstOrDefault(l => l != "Common");
            if (!string.IsNullOrEmpty(nonCommon))
                return nonCommon;

            if (mutual.Contains("Common"))
                return "Common";

            return null;
        }

        public static void GiveThought(Pawn p1, Pawn p2, ThoughtDef def)
        {
            p1?.needs?.mood?.thoughts?.memories?.TryGainMemory(def, p2);
        }
    }
}
