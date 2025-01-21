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
                    Log.Message($"[Rimguistics] {__instance.LabelShort} is learning a language...");
                    
                    string lang = LangUtils.AllLangs?.Where(l => __instance.Faction == l.BelongingFaction)?.FirstOrDefault()?.LangName ?? "Common";
                    langComp.SetLanguageSkill(lang, 500f);
                    // comp.SetLanguageSkill("Common", 1f); // Default to "Common" with a skill level of 1
                    Log.Message($"[Rimguistics] Assigned language \"{lang}\" to {__instance.LabelShort}.");
                }

                catch (Exception ex)
                {
                    Log.Error($"[Rimguistics] Unable to assign a language to {__instance.LabelShort}: " + ex.Message);
                }
            }


        }
    }
}

