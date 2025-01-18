using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LudeonTK;
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
            if ( __result == null || !__result.def.humanlikeFaction)
                return;

            string langName = "Common";

            if (!__result.IsPlayer)
            {

                try 
                {
                  

                    langName = NameGenerator.GenerateName(LangNamerDefOf.RandomLangNamer());
                        LangUtils.CreateLang(__result, langName);
                        Log.Message($"Created new language, \"{langName}\"" + (__result.IsPlayer ? "." : " for " + __result.Name + "."));
                }
                catch (Exception e)
                {
                    Log.Warning($"[Rimlingo] Unable to name the language for {__result.Name}: " + e.Message);
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
