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
            Scribe_Collections.Look(ref Languages, "TTDG.languageSkills");
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
