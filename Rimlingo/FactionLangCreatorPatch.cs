using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rimlingo
{
    [HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction")]
    internal class FactionLangCreatorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Faction __result)
        {
            if (!__result.def.humanlikeFaction)
                return;

            string langName = "Common";

            if (!__result.IsPlayer)
            {
                
                if (__result.def.allowedCultures.Any())
                {
                    //this is a really bad dumb way to to it but I'm sick of working on it so it's what we've got lol
                    langName = __result.Name.Split(' ').Where(x => x.Length > 3).FirstOrDefault(); ;
                        LangUtils.CreateLang(__result, langName);
                        Log.Message($"Created new language, \"{langName}\"" + (__result.IsPlayer ? "." : " for " + __result.Name + "."));

                }
                else
                {
                    Log.Message($"[Rimlingo] Unable to name the language for {__result.Name}.");
                }
            }
            else
            {
                LangUtils.CreateLang(null, langName);
                Log.Message($"Created new language, \"{langName}\"" + (__result.IsPlayer ? "." : " for " + __result.Name + "."));
            }
         
        }
    }
}
