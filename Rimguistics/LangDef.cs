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
        public string LangName;
        public Faction BelongingFaction;
        public LangDef(string langName, Faction belongingFaction) 
        {
            LangName = langName;
            BelongingFaction = belongingFaction;
        }

        public override string ToString()
        {
            return $"{LangName}, {BelongingFaction?.Name ?? "Null"}";// base.ToString();
        }
    }
}
