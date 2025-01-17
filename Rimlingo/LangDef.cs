using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimlingo
{
    public class LangDef : Def
    {
        public string Name;
        public Faction BelongingFaction;
        public LangDef(string langName, Faction belongingFaction) 
        {
            Name = langName;
            BelongingFaction = belongingFaction;
        }

    }
}
