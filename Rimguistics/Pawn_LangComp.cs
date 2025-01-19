using System.Collections.Generic;
using Verse;

namespace Rimguistics
{
    public class Pawn_LangComp : ThingComp
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
            //always learn at least 1% so pawns can learn even if pawn has 0 int 
            return languageSkills.TryGetValue(languageDefName, out float skill) ? skill : 1f;
        }

        public void SetLanguageSkill(string languageDefName, float value)
        {
            languageSkills[languageDefName] = value;
        }
    }
}
