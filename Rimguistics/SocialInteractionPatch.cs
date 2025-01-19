using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System;

namespace Rimguistics
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public static class SocialInteractionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_InteractionsTracker __instance, Pawn recipient, ref bool __result)
        {
            //retrieve the initiating pawn
            var field = __instance.GetType().GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            var initiatingPawn = field.GetValue(__instance) as Pawn;

            //if either participant is an animal, skip linguistics.
            if ((recipient.NonHumanlikeOrWildMan() && !recipient.IsWildMan()) || (initiatingPawn.NonHumanlikeOrWildMan() && !initiatingPawn.IsWildMan()))
            {
                Log.Message($"[Rimguistics] Animal interaction. No languages take place.");
                //run default code
                return true;
            }

            //should never happen, but if it does, skip linguistics and let default code handle it.
            if (initiatingPawn == null)
            {
                Log.Error("[Rimguistics] Initiating pawn was null");
                return true;
            }
            else
            {
                Log.Message($"[Rimguistics] {initiatingPawn.LabelShort} initiated an interaction with {recipient.LabelShort}.");
            }

            // Attempt best language
            string chosenLanguage = LangUtils.DetermineBestLanguage(initiatingPawn, recipient);
            if (string.IsNullOrEmpty(chosenLanguage))
            {
                DoLangLearningForNoSharedLangs(recipient, initiatingPawn);

                //if no common language is found the interaction fails and original code is skipped. Will have to test to make sure it doesn't break.
                Log.Message($"[Rimguistics] Interaction failed between {initiatingPawn} and {recipient}. No shared language.");
                MoteMaker.MakeInteractionBubble(initiatingPawn, recipient, LangInteractionsDefOf.LanguageInteractionFail.interactionMote, LangInteractionsDefOf.LanguageInteractionFail.GetSymbol(initiatingPawn.Faction, initiatingPawn.Ideo), LangInteractionsDefOf.LanguageInteractionFail.GetSymbolColor(initiatingPawn.Faction));

                //the interaction succeeded according to the game (if it doesn't it throws an error)
                __result = true;
                //return false to skip default code; the interaction failed because the pawns were unable to communicate.
                return false;
            }

            Log.Message($"[Rimguistics] \"{chosenLanguage}\" was chosen for the interaction.");

            //if Common is the only language, no benefits happen. Skip to default code.
            if (chosenLanguage == "Common")
            {
                Log.Message($"[Rimguistics] Interaction took place between {initiatingPawn} and {recipient} in Common.");
                //return true to run default code.
                __result = true;
                return true; 
            }


            //otherwise give special effect.

            float lowerSkill = Math.Min(LangUtils.GetLanguageSkill(recipient, chosenLanguage), LangUtils.GetLanguageSkill(initiatingPawn, chosenLanguage));
            bool initiatorSkillTooLow = LangUtils.GetLanguageSkill(initiatingPawn, chosenLanguage) < LangUtils.GetLanguageSkill(recipient, chosenLanguage);

            // Non-common => good thoughts & skill gain
            DoLanguageThoughts(recipient, initiatingPawn, chosenLanguage);

            DoLearning(recipient, initiatingPawn, chosenLanguage);



            //Log pawns from different factions interacted.
            if (initiatingPawn.def == recipient.def && initiatingPawn.Faction != recipient.Faction)
            {
                Log.Message($"[Rimguistics] {initiatingPawn.LabelShort} & {recipient.LabelShort} share species but different factions!");
            }

            if(lowerSkill < 25f)
            {
                string tooLowPawn = initiatorSkillTooLow ? initiatingPawn.LabelShort :  recipient.LabelShort;
                Log.Message($"[Rimguistics] Interaction tried to take place between {initiatingPawn} and {recipient} in {chosenLanguage}, but {tooLowPawn}'s skill was too low! ({lowerSkill})");
                //the interaction succeeded according to the game (if it doesn't it throws an error)
                __result = true;
                MoteMaker.MakeInteractionBubble(initiatingPawn, recipient, LangInteractionsDefOf.LanguageInteractionFail.interactionMote, LangInteractionsDefOf.LanguageInteractionFail.GetSymbol(initiatingPawn.Faction, initiatingPawn.Ideo), LangInteractionsDefOf.LanguageInteractionFail.GetSymbolColor(initiatingPawn.Faction));
                //return false to skip default code
                return false;
            }
            //Log interaction.
            Log.Message($"[Rimguistics] Interaction took place between {initiatingPawn} and {recipient} in {chosenLanguage}. Skill: {lowerSkill}");



            __result = true;
            return true; //return true to run default code
        }

        /// <summary>
        /// Performs the logic to apply language thoughts to <paramref name="initiatingPawn"/> and <paramref name="recipient"/> based on the lower of the pawns' skill in <paramref name="chosenLanguage"/>.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="initiatingPawn"></param>
        /// <param name="chosenLanguage"></param>
        private static void DoLanguageThoughts(Pawn recipient, Pawn initiatingPawn, string chosenLanguage)
        {
            //TODO: Make buffs only happen when speaking to a pawn not of initiator's faction.

            float lowerSkill = Math.Min(LangUtils.GetLanguageSkill(recipient, chosenLanguage), LangUtils.GetLanguageSkill(initiatingPawn, chosenLanguage));
            //Get langThought scaled with the lower score of the interacting pawns.
            var langThought = LangUtils.GetLangThoughtBasedOnFloat(lowerSkill);

            LangUtils.GiveThought(initiatingPawn, recipient, langThought);
            LangUtils.GiveThought(recipient, initiatingPawn, langThought);

        }

        private static void DoLearning(Pawn recipient, Pawn initiatingPawn, string chosenLanguage)
        {
            //TODO: Balance language learning so it's not done overnight. Need a formula... maybe something like LanguageSkill += Intelligence/20 min 1.
            //TODO: Unlearn languages that aren't being used.
            //TODO: only learn if the other pawn is better at the language than the other

            int intA = LangUtils.GetPawnIntelligence(initiatingPawn);
            int intB = LangUtils.GetPawnIntelligence(recipient);
            //If pawn is smarter than 0, increase langScore.
            if (intA > 0)
                LangUtils.AlterLanguageSkill(initiatingPawn, chosenLanguage, intA);
            if (intB > 0)
                LangUtils.AlterLanguageSkill(recipient, chosenLanguage, intB);
        }

        /// <summary>
        /// Performs the logic to apply the best known language of <paramref name="initiatingPawn"/> to <paramref name="recipient"/> and vice versa at skill level 0.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="initiatingPawn"></param>
        private static void DoLangLearningForNoSharedLangs(Pawn recipient, Pawn initiatingPawn)
        {
            Log.Message($"[Rimguistics] Teaching each pawn the other's most prominent with score 0.");

            var recLangComp = LangUtils.GetLanguagesComp(recipient);
            var initLangComp = LangUtils.GetLanguagesComp(initiatingPawn);

            var recLangs = recLangComp.languageSkills.Keys.ToList();
            recLangs.Sort();

            var initLangs = initLangComp.languageSkills.Keys.ToList();
            initLangs.Sort();

            var bestLangOfReceiver = recLangs.FirstOrDefault();
            var bestLangOfInitiator = initLangs.FirstOrDefault();

            if (recLangs.Any())
            {
                //recLangComp.SetLanguageSkill(initLangs.FirstOrDefault(), 0);
                LangUtils.AlterLanguageSkill(initiatingPawn, bestLangOfReceiver, 0);
            }

            if (initLangs.Any())
            {
                //initLangComp.SetLanguageSkill(recLangs.FirstOrDefault(), 0);
                LangUtils.AlterLanguageSkill(recipient, bestLangOfInitiator, 0);
            }


            //this has to happen after because if it doesn't it can't find any language skill of the prominent lang.
            LangUtils.GiveThought(initiatingPawn, recipient, DefDatabase<ThoughtDef>.GetNamed("LinguisticWall", true));
            LangUtils.GiveThought(recipient, initiatingPawn, DefDatabase<ThoughtDef>.GetNamed("LinguisticWall", true));


        }
    }
}