using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimlingo
{
    public static class LanguageUtility
    {
        // Retrieves the CompPawnLanguages component from a given pawn, which manages language skills.
        public static CompPawnLanguages GetLanguagesComp(Pawn pawn)
        {
            return pawn?.TryGetComp<CompPawnLanguages>();
        }

        /// <summary>
        /// Returns the language knowledge value of language with name <paramref name="langDefName"/> from <paramref name="pawn"/>
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <returns></returns>
        public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            return GetLanguagesComp(pawn)?.GetLanguageSkill(langDefName) ?? 0f;
        }

        // Increases the language skill of a pawn for a specific language by a given amount, capped at 100.
        public static void IncreaseLanguageSkill(Pawn pawn, string langDefName, float amount)
        {
            var comp = GetLanguagesComp(pawn);
            if (comp == null) return;

            float current = comp.GetLanguageSkill(langDefName);
            float newVal = Math.Min(current + amount, 100f);
            comp.SetLanguageSkill(langDefName, newVal);

            Log.Message($"[Rimlingo] {pawn.LabelShort} gained {amount:F1} in {langDefName}, now {newVal:F1}.");
        }

        // Retrieves the intelligence level of a pawn, which is used to determine language learning capability.
        public static int GetPawnIntelligence(Pawn pawn)
        {
            var skill = pawn?.skills?.GetSkill(SkillDefOf.Intellectual);
            return skill?.Level ?? 0;
        }

        // Determines the best mutual language between two pawns, prioritizing non-common languages.
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

        // Gives a thought to a pawn about another pawn, using a specified ThoughtDef.
        public static void GiveThought(Pawn p1, Pawn p2, ThoughtDef def)
        {
            p1?.needs?.mood?.thoughts?.memories?.TryGainMemory(def, p2);
        }
    }
}