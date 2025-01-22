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
public static class PawnSpawnPatch
{
    /// <summary>
    /// Adds a language component to <paramref name="__instance"/> and assigns starting languages.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="respawningAfterLoad"></param>
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, bool respawningAfterLoad)
    {
        // Check if the pawn is humanlike
        if (__instance.RaceProps.Humanlike)
        {
            Log.Message($"[Rimguistics] Humanlike: \"{__instance.LabelShort}\" is spawned");

            if (__instance.TryGetComp<Pawn_LangComp>() == null)
            {
                Log.Message($"[Rimguistics] {__instance.LabelShort} is getting a LangComp");

                // Force-add the comp
                var comp = new Pawn_LangComp();
                comp.parent = __instance;
                __instance.AllComps.Add(comp);
            }

            if (__instance.IsWildMan())
            {
                Log.Message("[Rimguistics] Not adding languages to wild man " + __instance.LabelShort);
                return;
            }

            var langComp = __instance.AllComps.OfType<Pawn_LangComp>().FirstOrDefault();
            if (!langComp.Languages.Any())
            {
                try
                {
                    int iPass = (int)__instance.skills.GetSkill(SkillDefOf.Intellectual).passion;
                    int sPass = (int)__instance.skills.GetSkill(SkillDefOf.Social).passion;
                    int maxAditionalLangs = 1 + iPass + sPass;
                    langComp.SetMaxLangs(maxAditionalLangs + 1);

                    int additionalLangs = maxAditionalLangs;// Rand.Range(0, maxAditionalLangs);
                    string useS = additionalLangs >= 0 ? "s" : "";
                    Log.Message($"[Rimguistics] {__instance.LabelShort} is learning {additionalLangs + 1} language{useS}...");
                    
                    string nativeLang = LangUtils.AllLangs?.Where(l => __instance.Faction == l.BelongingFaction)?.FirstOrDefault()?.LangName ?? "Common";
                    langComp.SetLanguageSkill(nativeLang, 500f);



                    for (int i = additionalLangs; i > 0; i--)
                    {

                        List<string> knownLangs = new List<string>();
                        foreach (var availableLang in langComp.Languages.Values)
                        {
                            knownLangs.Add(availableLang.LangName);
                        }

                        langComp.ListKnownLangs();

                        List<string> availableLangs = new List<string>();
                        foreach(var availableLang in LangUtils.AllLangs)
                        {
                            availableLangs.Add(availableLang.LangName);
                        }

                        availableLangs = availableLangs.Except(knownLangs).ToList();

                            //Except( knownLangs ).ToList();

                        Log.Message("available langs".Colorize(Color.magenta));
                        foreach (var thing in availableLangs)
                        {
                            Log.Message($"- {thing.ToString()}".Colorize(Color.magenta));
                        }
                        int languageCount = availableLangs.Count - 1;
                        languageCount = languageCount < 0 ? 0 : languageCount;

                        string learningLang = availableLangs[Rand.Range(0, languageCount)];

                        if (knownLangs.Where(kl => kl == learningLang).Any())
                        {
                            Log.Error($"[Rimguistics] {learningLang} has already been chosen");
                            continue;
                        }
                        langComp.SetLanguageSkill(learningLang, DetermineStartingKnowledge(__instance.skills.GetSkill(SkillDefOf.Intellectual).Level, __instance.skills.GetSkill(SkillDefOf.Social).Level));
                    }

                    if (!langComp.Languages.Values.Any()) Log.Error("[Rimguistics] No langs were assigned to " + __instance.LabelShort);

                    if (additionalLangs == 0) Log.Message($"[Rimguistics] No additional langs were assigned to {__instance.LabelShort}".Colorize(Color.green));

                    Log.Message("-------");

                    // comp.SetLanguageSkill("Common", 1f); // Default to "Common" with a skill level of 1
                }

                catch (Exception ex)
                {
                    Log.Error($"[Rimguistics] Unable to assign languages to {__instance.LabelShort}: " + ex.Message);
                }
            }
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

