using HarmonyLib;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace Rimguistics
{
    public class Pawn_LangComp : ThingComp
    {
        // Key = "German", "French", etc. Value = skill [0..100]
        public Dictionary<string, LangDef> Languages = new Dictionary<string, LangDef>();
        public int MaxLangs { get; private set; } = 1;

        public void SetMaxLangs(int maxLangs)
        {
            MaxLangs = maxLangs;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref Languages, "TTDG.languageSkills");
        }

        public float GetLanguageSkill(string languageDefName)
        {
            return Languages.TryGetValue(languageDefName, out LangDef lang) ? lang.Skill : 0f;
        }

        public bool SetLanguageSkill(string langName, float value)
        {
            var pawn = parent as Pawn;

            //Pawns can only learn a language if they have a language slot or already are learning it.
            if (Languages.Count < MaxLangs || Languages.Keys.Contains(langName))
            {

                if (!Languages.ContainsKey(langName))
                {
                    Languages.Add(langName, new LangDef(langName, value));
                    Languages[langName].SetSkill(value);
                }
                else
                {
                    Languages[langName].SetSkill(value);
                }

                if (Languages[langName].Skill < 0)
                {
                    Languages.Remove(langName); 
                    

                    RulePackDef def = new RulePackDef();

                    var interactionLostLang = LangInteractionsDefOf.LanguageLost;
                    interactionLostLang.logRulesInitiator.Rules.Add(new Rule_String("LOSTLANGUAGE", langName));
                    PlayLogEntry_LanguageLost playLogEntry = new PlayLogEntry_LanguageLost(interactionLostLang, pawn, langName);
                    Find.PlayLog.Add(playLogEntry);

                    Messages.Message($"{pawn.LabelShort} has lost the ability to speak {langName}", MessageTypeDefOf.NegativeEvent);


                    //Log.Message(parent.LabelShort + " lost the ability to speak " + langName);
                }
                return true;
            }
            else
            {
                Log.Warning(parent.LabelShort + " cannot learn " + langName);
                return false;
            }
        }

        public void ListKnownLangs()
        {
            Log.Message($"{parent.LabelShort}'s languages: ".Colorize(Color.cyan));
            foreach (var lang in Languages)
            {
                Log.Message($" - {lang.Value.ToString()}".Colorize(Color.cyan));
            }
        }
    }
}
