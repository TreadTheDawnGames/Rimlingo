using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rimguistics
{

 
    public class LangUtils : GameComponent
    { 
        
        public static List<LangDef> AllLangs = new List<LangDef> { };

        public LangUtils(Game game)
        {
            AllLangs = new List<LangDef>();
            Scribe_Collections.Look(ref AllLangs, "TTDG.Rimguistics.AllLangs");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref AllLangs, "TTDG.Rimguistics.AllLangs");

        }

        /// <summary>
        /// Retrieves the CompPawnLanguages component from <paramref name="pawn"/>, which manages language skills.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static Pawn_LangComp GetPawnLangComp(Pawn pawn)
        {
            return pawn?.TryGetComp<Pawn_LangComp>();
        }

        /// <summary>
        /// Returns the language knowledge score of <paramref name="pawn"/> for language <paramref name="langDefName"/>.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <returns></returns>
       /* public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            Log.Message("Getting Language Skill");
            return GetPawnLangComp(pawn)?.GetLanguageSkill(langDefName) ?? 0f;
        }*/
        
        public static float GetLanguageSkill(Pawn pawn, string langDefName)
        {
            return GetPawnLangComp(pawn)?.GetLanguageSkill(langDefName) ?? 0f;
        }

        /// <summary>
        /// Changes <paramref name="pawn"/>'s <paramref name="langDefName"/> knowledge score by <paramref name="amount"/>, capped at 500.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="langDefName"></param>
        /// <param name="amount"></param>
        public static void AlterLanguageSkill(Pawn pawn, string langDefName, float amount)
        {
            var comp = GetPawnLangComp(pawn);
            if (comp == null)
            {
                Log.Error($"{pawn.LabelShort} has no LangComp! Cannot alter {langDefName} skill.");
                return;
            }
            float current = comp.GetLanguageSkill(langDefName);
            float newVal = Math.Min(current + amount, 5000f);
            //newVal = newVal < 0 ? 0 : newVal;

            if(comp.SetLanguageSkill(langDefName, newVal)) 
                Log.Message($"[Rimguistics] {pawn.LabelShort} gained {amount} in {langDefName}, now {newVal}.");
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
        
        public static int GetPawnSocial(Pawn pawn)
        {
            var skill = pawn?.skills?.GetSkill(SkillDefOf.Social);
            return skill?.Level ?? 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public static float GetPawnLearningFactor(Pawn pawn)
        {

            bool debug = RimguisticsMod.Settings.useDebugValues;
            if(pawn == null)
            {
                Log.Error($"[Rimguistics] Unable to get learning factor: Pawn was null.");
                return 0f;
            }

            //MaybeRebalance: Balance language learning so it's not done overnight. Need a formula... maybe something like LanguageSkill += Intelligence/20 min 1.
            
            float intelligence = GetPawnIntelligence(pawn);
            float social = GetPawnSocial(pawn);

            //minimum learn is always 0.08666667..., max is 2.
            var intPassion = (int)pawn.skills.GetSkill(SkillDefOf.Intellectual).passion;
            var socPassion = (int)pawn.skills.GetSkill(SkillDefOf.Social).passion;
            double factor = ((Math.Max(social, 1f) / 15f ) + (Math.Max(intelligence, 1f) /5f) / (10f - intPassion - socPassion));


            if(debug)
            {
                intelligence = intelligence > 0 ? intelligence : 10;
            }

            return debug ? intelligence : (float)factor;


        }

        /// <summary>
        /// Determines the best mutual language between <paramref name="initiator"/> and <paramref name="recipient"/>, prioritizing non-common languages.
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        public static string GetBestLanguageName(Pawn initiator, Pawn recipient)
        {
            var compA = GetPawnLangComp(initiator);
            var compB = GetPawnLangComp(recipient);
            if (compA == null || compB == null)
            {
                string debugA = compA == null ? "null" : compA.ToString();
                string debugB = compB == null ? "null" : compB.ToString();
                Log.Error($"[Rimguistics] {initiator.LabelShort}'s language component was {debugA} and {recipient.LabelShort}'s was {debugB}.");
                return null;
            }
            var languagesA = compA.Languages.Where(kv => kv.Value.Skill >=0f).Select(kv => kv.Key).ToList();
            var languagesB = compB.Languages.Where(kv => kv.Value.Skill >=0f).Select(kv => kv.Key).ToList();

           /* Debug list for pawn languages
            * Log.Message("Langs A");
            foreach (var language in languagesA)
            {
                Log.Message(language);
            }
            Log.Message("Langs B");
            foreach (var language in languagesB)
            {
                Log.Message(language);
            }*/

            var mutual = languagesA.Intersect(languagesB).ToList();
            if (!mutual.Any())
            {
                Log.Message($"[Rimguistics] {initiator.LabelShort} and {recipient.LabelShort} share no languages!".Colorize(Color.green));
                return null;
            }
            //This should return the language they both have the highest score in, not necessarily the first or default. Doing so will mean pawns will have default langs not based on the ones they know the best.
            
            //TODO: Return language with highest skill combined(!?). This will determine which language is best known between both pawns.
            //      if pawn A has common 100 and french 10 and pawn B has common 0 and french 100, the scores are common 100 and french 110, so french is the chosen language.
            //      if pawn A has common 50 and french 10 and pawn B has common 10 and french 50, the scores are common 60 and french 60, so one is chosen at random. (Unless there is a preferred language (not implemented yet), in which case that language is used.)
            
            //      Languages could have a preference score equal to skill + preferenceScore (+10, +5, +0, -5, or -10). Instead of comparing raw skill, compare preference score.
            
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
            return GetPawnLangComp(pawn).GetLanguageSkill(langDef) > 0;
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
                    LangUtils.AllLangs.Add(new LangDef(faction.Name + "-ese", faction));
                    Log.Warning($"[Rimguistics] langName was null. Using fallback.");

                }
                else
                {
                    LangUtils.AllLangs.Add(new LangDef("The Impossible Language", faction));

                    Log.Error($"[Rimguistics] Faction name was null. Using The impossible language has appeared!");
                }
            }
            else
            {
                LangUtils.AllLangs.Add(new LangDef(langName, faction));
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
        public static ThoughtDef GetLangThoughtBasedOnFloat(float input)
        {
            Log.Message("Finished Getting Language Skill");

            int skill = (int)input;

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
                        Log.Error("Default case hit in GetLangThoughtBasedOnFloat(float). Skill: " + skill);
                        return null;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Rimguistics] Unable to get lang thought base on pawn skill: " + e.Message + $" | skill: {skill}");
                return null;
            }



        }

        public static void RemoveLangFromPawn(Pawn pawn, string lang)
        {
            try
            {
                if (GetPawnLangComp(pawn).Languages.Remove(lang))
                {

                    PlayLogEntry_LanguageLost playLogEntry = new PlayLogEntry_LanguageLost(LangInteractionsDefOf.LanguageLost, pawn, lang);
                    Find.PlayLog.Add(playLogEntry);
                    Messages.Message($"{pawn.LabelShort} has lost the ability to speak {lang}", MessageTypeDefOf.NegativeEvent);
                }

            }
            catch
            {
                Log.Message($"[Rimguistics] Unable to remove {lang ?? "Unknown"} from {pawn.LabelShort ?? "Pawn"}.");
            }
        }
    }

    public class LangComparer_Skill : IComparer<LangDef>
    {
        public int Compare(LangDef x, LangDef y)
        {
            return x.Skill > y.Skill ? 1 : 0 ;
        }
    }

}