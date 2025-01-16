using System.Collections.Generic;
using Verse;

namespace Rimlingo
{
    public class CompPawnLanguages : ThingComp
    {
        // Key = "German", "French", etc. Value = skill [0..100]
        public Dictionary<string, float> languageSkills = new Dictionary<string, float>();

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref languageSkills, "languageSkills", LookMode.Value, LookMode.Value);
        }

        public float GetLanguageSkill(string languageDefName)
        {
            return languageSkills.TryGetValue(languageDefName, out float skill) ? skill : 0f;
        }

        public void SetLanguageSkill(string languageDefName, float value)
        {
            languageSkills[languageDefName] = value;
        }
    }
}
