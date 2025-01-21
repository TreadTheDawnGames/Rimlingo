using HarmonyLib;
using Rimguistics;
using RimWorld;
using System;
using System.Linq;
using Verse;
using static Unity.Burst.Intrinsics.X86.Avx;

[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
public static class PawnSpawnPatch
{
    /// <summary>
    /// Adds a language component to <paramref name="__instance"/>.
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

            var langComp = __instance.AllComps.OfType<Pawn_LangComp>().FirstOrDefault();
            if (!langComp.Languages.Any())
            {

                try
                {
                    int iPass = (int)__instance.skills.GetSkill(SkillDefOf.Intellectual).passion;
                    int sPass = (int)__instance.skills.GetSkill(SkillDefOf.Social).passion;
                    int maxLangs = 1 + iPass + sPass;
                    int additionalLangs = Rand.Range(1, maxLangs);

                    string useS = additionalLangs > 0 ? "s" : "";
                    Log.Message($"[Rimguistics] {__instance.LabelShort} is learning {additionalLangs + 1} language{useS}...");
                    
                    string nativeLang = LangUtils.AllLangs?.Where(l => __instance.Faction == l.BelongingFaction)?.FirstOrDefault()?.LangName ?? "Common";
                    langComp.SetLanguageSkill(nativeLang, 500f);

                    for (int i = additionalLangs; i > 0; i--)
                    {
                        int maxVal = LangUtils.AllLangs.Count - 1;

                        LangDef[] knownLangs = langComp.Languages.Values.ToArray();
                        maxVal = maxVal < 0 ? 0 : maxVal;
                        string learningLang = LangUtils.AllLangs?.Where(l => !knownLangs.Contains(l) && l.BelongingFaction != Find.FactionManager.OfAncients).ToList()[Rand.Range(0,maxVal)].LangName;
                        langComp.SetLanguageSkill(learningLang, Rand.Range(0, 300));
                    }

                        Log.Message($"[Rimguistics]  {__instance.LabelShort}'s Languages:.");
                    foreach(var l in langComp.Languages.Values)
                    {
                        Log.Message(l.ToString());
                    }

                    if (!langComp.Languages.Values.Any()) Log.Error("[Rimguistics] No langs were assigned to " + __instance.LabelShort);

                    // comp.SetLanguageSkill("Common", 1f); // Default to "Common" with a skill level of 1
                }

                catch (Exception ex)
                {
                    Log.Error($"[Rimguistics] Unable to assign a language to {__instance.LabelShort}: " + ex.Message);
                }
            }


        }
    }
}

