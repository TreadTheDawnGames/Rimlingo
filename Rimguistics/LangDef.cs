using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimguistics
{
    public class LangDef : Def
    {
        public enum FluencyState { Native, Fluent, Foreign}
        public string LangName;
        public Faction BelongingFaction;
        public float Skill;
        public FluencyState Fluency;
        public LangDef(string langName, Faction belongingFaction) 
        {
            LangName = langName;
            BelongingFaction = belongingFaction;
        }
        
        public LangDef(string langName, float skill) 
        {
            LangName = langName;
            Skill = skill;
            DetermineFluency();
            
        }

        public void DetermineFluency()
        {
            if (Skill < 100)
            {
                Fluency = FluencyState.Foreign;
            }
            else if (Skill >= 100 && Skill < 400)
            {
                Fluency = FluencyState.Fluent;
            }
            else if (Skill >= 400)
            {
                Fluency = FluencyState.Native;
            }
        }

        public void SetSkill(float skill)
        {
            Skill = skill;
            DetermineFluency();
        }

        public override string ToString()
        {
            return $"{LangName}, {Skill.ToString() + ", " ?? ""}{Fluency.ToString() + ", " ?? ""}{BelongingFaction?.Name ?? ""}";
        }
    }
}
