using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimguistics
{
    public static class LangUtils
    {
        //ToDo:
        //get actual list of langs. Could get from actual langs (rimworld/languages), or from a langName.xml

        public static List<LangDef> AllLangs = new List<LangDef> { };


        /// <summary>
        /// Retrieves the CompPawnLanguages component from <paramref name="pawn"/>, which manages language skills.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static Pawn_LangComp GetLanguagesComp(Pawn pawn)
        {
            return pawn?.TryGetComp<Pawn_LangComp>();
        }

        /// <summary>
        /// Returns the language knowledge score of <paramref name="pawn"/> for language <paramref name="langDefName"/>.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <returns></returns>
        public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            Log.Message("Getting Language Skill");
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

            Log.Message($"[Rimguistics] {pawn.LabelShort} gained {amount:F1} in {langDefName}, now {newVal:F1}.");
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
                Log.Error($"[Rimguistics] {initiator.LabelShort}'s language component was {debugA} and {recipient.LabelShort}'s was {debugB}.");
                return null;
            }
            var languagesA = compA.languageSkills.Where(kv => kv.Value >=0f).Select(kv => kv.Key).ToList();
            var languagesB = compB.languageSkills.Where(kv => kv.Value >=0f).Select(kv => kv.Key).ToList();

            Log.Message("Langs A");
            foreach (var language in languagesA)
            {
                Log.Message(language);
            }
            Log.Message("Langs B");
            foreach (var language in languagesB)
            {
                Log.Message(language);
            }

            var mutual = languagesA.Intersect(languagesB).ToList();
            if (!mutual.Any())
            {
                Log.Message($"[Rimguistics] {initiator.LabelShort} and {recipient.LabelShort} share no languages!".Colorize(Color.green));
                return null;
            }
            //This should return the language they both have the highest score in, not necessarily the first or default. Doing so will mean pawns will have default langs not based on the ones they know the best.
            var nonCommon = mutual.FirstOrDefault(l => l != "Common");
            if (!string.IsNullOrEmpty(nonCommon))
            {
                Log.Message($"[Rimguistics] \"{nonCommon}\" is the best language to use.");
                return nonCommon;
            }

            if (mutual.Contains("Common"))
            {
                Log.Message($"[Rimguistics] \"Common\" is the best language to use.");

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
            //1/16/2025 Worth trying this:
            //Pawn_InteractionsTracker.AddInteractionThought(rememberingPawn, memoryPawn, def);
        }

        /// <summary>
        /// Returns whether <paramref name="pawn"/> knows <paramref name="langDef"/>
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDef"></param>
        /// <returns></returns>
        public static bool PawnKnowsLanguage(Pawn pawn, string langDef)
        {
            return GetLanguagesComp(pawn).GetLanguageSkill(langDef) > 0;
        }

        
        /// <summary>
        /// Create a language with name <paramref name="langName"/> and make the factin that speaks it <paramref name="faction"/>
        /// </summary>
        /// <param name="faction"></param>
        /// <param name="langName"></param>
        public static void CreateLang(Faction faction, string langName = null)
        {
            if(langName == null)
            {
                if(faction.Name!=null)
                {
                    AllLangs.Add(new LangDef(faction.Name + "-ese", faction));
                    Log.Warning($"[Rimguistics] langName was null. Using fallback.");

                }
                else
                {
                    AllLangs.Add(new LangDef("The Impossible Language", faction));

                    Log.Error($"[Rimguistics] Faction name was null. Using The impossible language has appeared!");
                }
            }
            else
            {
                AllLangs.Add(new LangDef(langName, faction));
            }
        }

        /// <summary>
        /// Returns a thought scaled with <paramref name="pawn"/>'s skill in <paramref name="lang"/>.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static ThoughtDef GetLangThoughtBasedOnPawnSkill(Pawn pawn, string lang)
        {
            if(lang==null)
            {
                Log.Error("[Rimguistics] lang is null! Unable to retrieve lang skill");
            }

            float langSkill = GetLanguageSkill(pawn, lang);
            Log.Message("Finished Getting Language Skill");

            try
            {

                switch (langSkill % 8)
                {
                    case 0:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticWall", true);
                    case 1:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true);
                    case 2:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyConfused", true);
                    case 3:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticMistake", true);
                    case 4:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticOptimism", true);
                    case 5:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticSuccess", true);
                    case 6:
                        return DefDatabase<ThoughtDef>.GetNamed("SharedLanguageBuff", true);
                    case 7:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Rimguistics] Unable to get lang thought base on pawn skill: " + e.Message + $" | {lang} skill: {langSkill}");
                return null;
            }



        }
        
        /// <summary>
        /// Returns a thought scaled with <paramref name="skill"/>
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static ThoughtDef GetLangThoughtBasedOnFloat(float skill)
        {
            Log.Message("Finished Getting Language Skill");

            try
            {

                switch (skill % 8)
                {
                    case 0:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticWall", true);
                    case 1:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true);
                    case 2:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyConfused", true);
                    case 3:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticMistake", true);
                    case 4:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticOptimism", true);
                    case 5:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticSuccess", true);
                    case 6:
                        return DefDatabase<ThoughtDef>.GetNamed("SharedLanguageBuff", true);
                    case 7:
                        return DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Rimguistics] Unable to get lang thought base on pawn skill: " + e.Message + $" | skill: {skill}");
                return null;
            }



        }

        [DebugAction("Rimguistics", "Teach Language", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TeachLanguage(Pawn pawn)
        {
           // Log.Message("Teaching Language");
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (LangUtils.AllLangs.Count <= 0)
            {
                list.Add(new FloatMenuOption("No langs", () => { Log.Message("[Rimguistics] No langs to teach!"); }));
                return;
            }

            for (int i = 0; i < LangUtils.AllLangs.Count; i++)
            {
                LangDef lang = LangUtils.AllLangs[i];
                list.Add(new FloatMenuOption($"{lang.LangName}, {lang.BelongingFaction}", delegate
                {
                    foreach (Pawn item in UI.MouseCell().GetThingList(Find.CurrentMap).OfType<Pawn>()
                        .ToList())
                    {
                        if (!item.RaceProps.Humanlike || LangUtils.PawnKnowsLanguage(item, lang.LangName))
                        {
                            break;
                        }
                        LangUtils.AlterLanguageSkill(item, lang.LangName, 100f);
                        DebugActionsUtility.DustPuffFrom(item);
                        Log.Message($"{pawn.LabelShort} now knows {lang} with score {LangUtils.GetLanguageSkill(pawn, lang.LangName)}");
                    }
                }));
            }
            Find.WindowStack.Add(new FloatMenu(list));

        }

        [DebugOutput(category = "Rimguistics", name = "List all Languages", onlyWhenPlaying = false)]
        public static void ListAllLangs()
        {
            foreach(var lang in AllLangs)
            {
                Log.Message(lang.ToString());
            }
        }
    }
}