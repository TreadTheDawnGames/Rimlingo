using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace Rimguistics
{
    internal class PlayLogEntry_LanguageLost : PlayLogEntry_InteractionSinglePawn
    {
        string LostLanguage = "";

        public PlayLogEntry_LanguageLost(InteractionDef intDef, Pawn initiator, string languageName)
        {
            this.intDef = intDef;
            this.initiator = initiator;
            initiatorFaction = initiator.Faction;
            initiatorIdeo = initiator.Ideo;
            LostLanguage = languageName;
        }

        // Override GenerateGrammarRequest to inject the lost language string
        protected override GrammarRequest GenerateGrammarRequest()
        {
            GrammarRequest request = base.GenerateGrammarRequest();

            request.Rules.Add(new Rule_String("LOSTLANGUAGE", LostLanguage));
            return request;


        }

        //blatant plagiarism
        protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
        {
            if (initiator == null)
            {
                Log.ErrorOnce("PlayLogEntry_InteractionSinglePawn has a null pawn reference.", 34422);
                return "[" + intDef.label + " error: null pawn reference]";
            }
            Rand.PushState();
            Rand.Seed = logID;
            GrammarRequest request = GenerateGrammarRequest();
            string text;
            if (pov == initiator)
            {
                request.IncludesBare.Add(intDef.logRulesInitiator);
                request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, request.Constants));
                text = GrammarResolver.Resolve("r_logentry", request, "interaction from initiator", forceLog);
            }
            else
            {
                Log.ErrorOnce("Cannot display PlayLogEntry_InteractionSinglePawn from POV who isn't initiator.", 51251);
                text = ToString();
            }
            Rand.PopState();
            return text;
        }
    }
}
