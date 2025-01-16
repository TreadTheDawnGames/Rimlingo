using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;

namespace Rimlingo
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public static class SocialInteractionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_InteractionsTracker __instance, Pawn recipient, ref bool __result)
        {
            // Retrieve initiator from private field "pawn"
            var initiator = __instance.GetType()
                .GetProperty("pawn", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(__instance) as Pawn;
            if (initiator == null)
                return true;

            // Attempt best language
            string chosenLanguage = LanguageUtility.DetermineBestLanguage(initiator, recipient);
            if (string.IsNullOrEmpty(chosenLanguage))
            {
                LanguageUtility.GiveThought(initiator, recipient,
                    DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true));
                LanguageUtility.GiveThought(recipient, initiator,
                    DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true));
                return true;
            }

            if (chosenLanguage == "Common")
                return true; // No special effect

            // Non-common => good thoughts & skill gain
            LanguageUtility.GiveThought(initiator, recipient,
                DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true));
            LanguageUtility.GiveThought(recipient, initiator,
                DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true));

            int intA = LanguageUtility.GetPawnIntelligence(initiator);
            int intB = LanguageUtility.GetPawnIntelligence(recipient);
            if (intA > 0)
                LanguageUtility.AlterLanguageSkill(initiator, chosenLanguage, intA);
            if (intB > 0)
                LanguageUtility.AlterLanguageSkill(recipient, chosenLanguage, intB);

            if (initiator.def == recipient.def && initiator.Faction != recipient.Faction)
            {
                Log.Message($"[Rimlingo] {initiator.LabelShort} & {recipient.LabelShort} share species but different factions!");
            }

            return true;
        }
    }
}