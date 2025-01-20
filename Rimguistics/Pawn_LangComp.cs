using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Rimguistics
{
    public class Pawn_LangComp : ThingComp
    {
        // Key = "German", "French", etc. Value = skill [0..100]
        public Dictionary<string, LangDef> Languages = new Dictionary<string, LangDef>();

        public override void PostExposeData()
        {

            base.PostExposeData();
/*
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Message("Saving".Colorize(Color.gray));
                var langList = Languages.Values.ToList();
                foreach (var lang in langList)
                {
                    Log.Message(lang.ToString());
                }

            }*/

                Scribe_Collections.Look(ref Languages, "TTDG.languageSkills");

            /*if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Message("PostLoadInit".Colorize(Color.blue));
                List<LangDef> langList = new List<LangDef> { };
                Dictionary<string, LangDef> langDict = new Dictionary<string, LangDef>();
                Scribe_Collections.Look(ref langList, "TTDG.languageSkills");
                foreach (var lang in langList)
                {
                    Log.Message(lang.ToString());
                    langDict.Add(lang.LangName, lang);
                }
                Languages = langDict;
            }*/
        }

        public float GetLanguageSkill(string languageDefName)
        {
            return Languages.TryGetValue(languageDefName, out LangDef lang) ? lang.Skill : 0f;
        }

        public void SetLanguageSkill(string langName, float value)
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
        }

        
    }
}
