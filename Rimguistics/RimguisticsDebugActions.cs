﻿using LudeonTK;
using Rimguistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AdventOfCode
{
    public static class RimguisticsDebugActions
    {

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

        [DebugAction("Rimguistics", "Unlearn all Languages", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void UnlearnLanguages(Pawn pawn)
        {
            if (pawn.NonHumanlikeOrWildMan())
                return;

            foreach(var lang in LangUtils.AllLangs)
                LangUtils.RemoveLangFromPawn(pawn, lang);

            DebugActionsUtility.DustPuffFrom(pawn);

        }

        [DebugAction("Rimguistics", "Learn first lang from AllLangs (using pawn intelligence)", allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMapForPawns)]
        public static void LearnFirstLanguage(Pawn pawn)
        {
            
            var lang = LangUtils.AllLangs.First();

            
                if (!pawn.RaceProps.Humanlike)
                {
                Log.Message($"[Rimguistics] {pawn.LabelShort} cannot learn a language.");
                    return;
                }
                LangUtils.AlterLanguageSkill(pawn, lang.LangName, LangUtils.GetPawnLearningFactor(pawn));
                DebugActionsUtility.DustPuffFrom(pawn);
                Log.Message($"{pawn.LabelShort} now knows {lang.LangName} with score {LangUtils.GetLanguageSkill(pawn, lang.LangName)}");
            


        }
        
        
        [DebugAction("Rimguistics", "Unlearn + Learn All/First Lang(s)", allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMapForPawns)]
        public static void LearnLanguage(Pawn pawn)
        {
            UnlearnLanguages(pawn);
            LearnFirstLanguage(pawn);
        }

        [DebugOutput(category = "Rimguistics", name = "List all Languages", onlyWhenPlaying = false)]
        public static void ListAllLangs()
        {
            if (!LangUtils.AllLangs.Any())
                Log.Error("[Rimguistics] No langs in AllLangs!");
            foreach (var lang in LangUtils.AllLangs)
            {
                Log.Message(lang.ToString());
            }
            Log.TryOpenLogWindow();
        }

        [DebugAction("Rimguistics", "List all pawn's languages", allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMapForPawns)]
        public static void ListPawnLangs(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike)
            {
                Log.Message(pawn.LabelShort + ": ");
                if (LangUtils.GetPawnLangComp(pawn).Languages.Any())
                    foreach (var lang in LangUtils.GetPawnLangComp(pawn).Languages)
                    {
                        Log.Message(lang.Value.ToString());
                    }
                else
                    if (!pawn.IsWildMan())
                    Log.Warning("No languages!");
                else
                    Log.Message("No languages.");
                Log.TryOpenLogWindow();
            }
        }
        
        [DebugAction("Rimguistics", "Unlearn best language (10)", allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMapForPawns)]
        public static void UnlearnLanguage(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike)
            {
                var langs = LangUtils.GetPawnLangComp(pawn).Languages;
                if (langs.Any())
                {
                    var langList = langs.Values.ToList();
                    LangDef lang = langList.MaxBy(item => item.Skill);

                    if(lang!=null)
                    {
                        LangUtils.AlterLanguageSkill(pawn, lang.LangName, -10f);
                        DebugActionsUtility.DustPuffFrom(pawn);
                    }
                    else
                    {
                        Log.Error("[Rimguistics] Unable to decrease "+lang.LangName+" skill");
                    }
                }
                else if (!pawn.IsWildMan())
                    Log.Warning("No languages!");
                else
                    Log.Message("No languages.");
            }
        }




    }
}
