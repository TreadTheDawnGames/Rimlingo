using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Rimlingo
{
    public static class LanguageUtility
    {
        /// <summary>
        /// Retrieves the CompPawnLanguages component from <paramref name="pawn"/>, which manages language skills.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static CompPawnLanguages GetLanguagesComp(Pawn pawn)
        {
            return pawn?.TryGetComp<CompPawnLanguages>();
        }

        /// <summary>
        /// Returns the language knowledge score of <paramref name="pawn"/> for language <paramref name="langDefName"/>.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <returns></returns>
        public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            return GetLanguagesComp(pawn)?.GetLanguageSkill(langDefName) ?? 0f;
        }

        /// <summary>
        /// Changes <paramref name="pawn"/>'s <paramref name="langDefName"/> knowledge score by <paramref name="amount"/>, capped at 100.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <param name="amount"></param>
        public static void AlterLanguageSkill(Pawn pawn, string langDefName, float amount)
        {
            var comp = GetLanguagesComp(pawn);
            if (comp == null)
            {
                Log.Error($"{pawn.LabelShort} has no LangComp! Cannot alter {langDefName} skill.");
                return;
            }
            float current = comp.GetLanguageSkill(langDefName);
            float newVal = Math.Min(current + amount, 100f);
            comp.SetLanguageSkill(langDefName, newVal);

            Log.Message($"[Rimlingo] {pawn.LabelShort} gained {amount:F1} in {langDefName}, now {newVal:F1}.");
        }

        /// <summary>
        /// Returns the intelligence level of <paramref name="pawn"/>, which is used to determine language learning capability.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static int GetPawnIntelligence(Pawn pawn)
        {
            var skill = pawn?.skills?.GetSkill(SkillDefOf.Intellectual);
            return skill?.Level ?? 0;
        }

        /// <summary>
        /// Determines the best mutual language between <paramref name="initiator"/> and <paramref name="recipient"/>, prioritizing non-common languages.
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        public static string DetermineBestLanguage(Pawn initiator, Pawn recipient)
        {
            var compA = GetLanguagesComp(initiator);
            var compB = GetLanguagesComp(recipient);
            if (compA == null || compB == null)
            {
                string debugA = compA == null ? "null" : compA.ToString();
                string debugB = compB == null ? "null" : compB.ToString();
                Log.Error($"[Rimlingo] {initiator.LabelShort}'s language component was {debugA} and {recipient.LabelShort}'s was {debugB}.");
                return null;
            }
            var languagesA = compA.languageSkills.Where(kv => kv.Value > 1f).Select(kv => kv.Key).ToList();
            var languagesB = compB.languageSkills.Where(kv => kv.Value > 1f).Select(kv => kv.Key).ToList();

            Log.Message("Langs A");
            foreach( var language in languagesA )
            {
                Log.Message(language);
            }
            Log.Message("Langs B");
            foreach( var language in languagesB )
            {
                Log.Message(language);
            }

            var mutual = languagesA.Intersect(languagesB).ToList();
            if (!mutual.Any())
            {
                Log.Error($"[Rimlingo] {initiator.LabelShort} and {recipient.LabelShort} share no languages!");
                return null;
            }
            //This should return the language they both have the highest score in, not necessarily the first or default. Doing so will mean pawns will have default langs not based on the ones they know the best.
            var nonCommon = mutual.FirstOrDefault(l => l != "Common");
            if (!string.IsNullOrEmpty(nonCommon))
            {
                Log.Message($"[Rimlingo] \"{nonCommon}\" is the best language to use.");
                return nonCommon;
            }

            if (mutual.Contains("Common"))
            {
                Log.Message($"[Rimlingo] \"Common\" is the best language to use.");

                return "Common";
            }

            return null;
        }

        /// <summary>
        /// Gives <paramref name="rememberingPawn"/> a thought about <paramref name="memoryPawn"/> using <paramref name="def"/>.
        /// </summary>
        /// <param name="rememberingPawn"></param>
        /// <param name="memoryPawn"></param>
        /// <param name="def"></param>
        public static void GiveThought(Pawn rememberingPawn, Pawn memoryPawn, ThoughtDef def)
        {
  
            rememberingPawn?.needs?.mood?.thoughts?.memories?.TryGainMemory(def, memoryPawn);
            //1/16/2025 Worth trying this: Pawn_InteractionsTracker.AddInteractionThought(rememberingPawn, memoryPawn, def);
        }
    }
}