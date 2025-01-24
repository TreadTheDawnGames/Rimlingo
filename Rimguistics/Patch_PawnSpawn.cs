using HarmonyLib;
using Rimguistics;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Unity.Burst.Intrinsics.X86.Avx;

[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
public static class Patch_PawnSpawn
{
    /// <summary>
    /// Adds a language component to <paramref name="__instance"/> and assigns starting languages.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="respawningAfterLoad"></param>
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, bool respawningAfterLoad)
    {
        //only do this setup on initial game start.
        if (respawningAfterLoad)
            return;

        // Check if the pawn is humanlike
        if (__instance.RaceProps.Humanlike)
        {   //debug
            //Log.Message($"[Rimguistics] Humanlike: \"{__instance.LabelShort}\" is spawned");
            
            //give pawn a langComp
            TryAddLangComp(__instance);

            //skill adding langs if wild man.
            if (__instance.IsWildMan())
            {
                //debug
                Log.Message("[Rimguistics] Not adding languages to wild man " + __instance.LabelShort);
                return;
            }

            var langComp = __instance.AllComps.OfType<Pawn_LangComp>().FirstOrDefault();
            if (langComp != null && !langComp.Languages.Any())
            {
                try
                {
                    //setup additional lang count.
                    int iPass = (int)__instance.skills.GetSkill(SkillDefOf.Intellectual).passion;
                    int sPass = (int)__instance.skills.GetSkill(SkillDefOf.Social).passion;
                    int maxAdditionalLangs = 1 + iPass + sPass;
                    langComp.SetMaxLangs(maxAdditionalLangs + 1);
                    int additionalLangs = RimguisticsMod.Settings.useDebugValues ? maxAdditionalLangs : Rand.Range(0, maxAdditionalLangs);

                    float intSkill = __instance.skills.GetSkill(SkillDefOf.Intellectual).Level;
                    float socSkill = __instance.skills.GetSkill(SkillDefOf.Social).Level;


                    //debug show how many langs are being learned.
                    string useS = additionalLangs >= 0 ? "s" : "";
                    Log.Message($"[Rimguistics] {__instance.LabelShort} is learning {additionalLangs + 1} language{useS}...");

                    //add native language with score 500.
                    string nativeLang = LangUtils.AllLangs?.Where(l => __instance.Faction == l.BelongingFaction)?.FirstOrDefault()?.LangName ?? "Common";
                    langComp.SetLanguageSkill(nativeLang, 500f);

                    if (Rand.Bool)
                    {
                        if (!LangUtils.PawnKnowsLanguage(__instance, "Common"))
                        {
                            additionalLangs -= 1;
                            langComp.SetLanguageSkill("Common", DetermineStartingKnowledge(intSkill, socSkill));
                        }
                    }

                    //setup additional langs
                    for (int i = additionalLangs; i > 0; i--)
                    {
                        //get list of known langs.
                        List<string> knownLangs = new List<string>();
                        foreach (var availableLang in langComp.Languages.Values)
                        {
                            knownLangs.Add(availableLang.LangName);
                        }

                        //debug 
                        //langComp.ListKnownLangs();

                        //get list for allLangs
                        List<string> unlearnedLangs = new List<string>();
                        foreach (var availableLang in LangUtils.AllLangs)
                        {
                            unlearnedLangs.Add(availableLang.LangName);
                        }

                        //filter all langs to only the langs pawn doesn't know.
                        unlearnedLangs = unlearnedLangs.Except(knownLangs).ToList();

                        //debug
                        /*Log.Message("available langs".Colorize(Color.magenta));
                        foreach (var thing in availableLangs)
                        {
                            Log.Message($"- {thing.ToString()}".Colorize(Color.magenta));
                        }*/
                        
                        //ensure index out of range doesn't occur when picking random language
                        int unlearnedLangCount = unlearnedLangs.Count - 1;
                        unlearnedLangCount = unlearnedLangCount < 0 ? 0 : unlearnedLangCount;

                        //pick random language from the list of non-learned langs
                        string learningLang = unlearnedLangs[Rand.Range(0, unlearnedLangCount)];

                        //if the language has already been learned, skill setting it up (should never happen)
                        if (knownLangs.Where(kl => kl == learningLang).Any())
                        {
                            Log.Error($"[Rimguistics] {learningLang} has already been chosen");
                            continue;
                        }

                        //finally, set the skill based on int and social.
                        langComp.SetLanguageSkill(learningLang, DetermineStartingKnowledge(intSkill, socSkill));
                    }

                    //if no languages were added, log an error. Languages should've been added.
                    if (!langComp.Languages.Values.Any()) Log.Error("[Rimguistics] No langs were assigned to " + __instance.LabelShort);

                    //debug
                    if (additionalLangs == 0) Log.Message($"[Rimguistics] No additional langs were assigned to {__instance.LabelShort}".Colorize(Color.green));
                    //Log.Message("-------");

                    if (__instance.Faction != Find.FactionManager.OfPlayer) langComp.SetPreferredLanguage(nativeLang);
                }

                catch (Exception ex)
                {
                    Log.Error($"[Rimguistics] Unable to assign languages to {__instance.LabelShort}: " + ex.Message);
                }
            }
            else
            {
                Log.Error($"[Rimguistics] {__instance.LabelShort}'s LangComp was either null or already had languages:\n LangComp: {langComp.ToString()}"+(langComp!=null?$"\n Language count: {langComp.Languages.Any()})" : ""));
            }
        }
    }

    private static void TryAddLangComp(Pawn __instance)
    {
        try
        {
            //if the langComp doesn't exist, add it.
            if (__instance.TryGetComp<Pawn_LangComp>() == null)
            {
                //debug
                Log.Message($"[Rimguistics] {__instance.LabelShort} is getting a LangComp");

                // Force-add the comp
                var comp = new Pawn_LangComp();
                comp.parent = __instance;
                __instance.AllComps.Add(comp);
            }
        }
        catch (Exception e)
        {
            Log.Error("[Rimguistics] Unable to give " + __instance.LabelShort + " a LangComp: " + e.Message);
        }
    }

    static float DetermineStartingKnowledge(float intSkill, float socSkill)
    {
        Log.Message(intSkill +" "+ socSkill);
        float maxKnow = Math.Max(intSkill, 1) + Math.Max(socSkill, 1) * 15;
        float knowledge = Rand.Range(5, maxKnow);

        return knowledge;
    }

}

