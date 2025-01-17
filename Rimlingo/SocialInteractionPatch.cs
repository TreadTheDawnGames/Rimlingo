using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;
using System.Linq;

namespace Rimlingo
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public static class SocialInteractionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_InteractionsTracker __instance, Pawn recipient, ref bool __result)
        {
            //this is not retrieving anything. "Pawn" doesn't exist. I don't know why or how. Unless the Pawn_InteractionsTracker(Pawn) doesn't assign Pawn to anything and just calls its functions in the constructor.
            // Retrieve initiator from private field "pawn"
            var field  = __instance.GetType().GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            Pawn p = field.GetValue(__instance) as Pawn;
            Log.Message(p.Name);

            /*foreach(var member in memberInfoArray)
            {
                Log.Message(member.);

                

                if(member.Name == "pawn")
                {
                    Log.Message("HERE'S THE PAWN");
                }
            }
*/
            var initiatingPawn = p;
                /*__instance.GetType()
                .GetProperty("pawn", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(__instance) as Pawn;*/


            /*var properties = 

            foreach ( var property in properties )
            {
                Log.Message(property.Name);
            }*/

            //should never happen
            if (initiatingPawn == null)
            {
                Log.Error("[Rimlingo] Initiating pawn was null");
                //run normal code anyway.
                return true;
            }
            else
            {
                Log.Message($"[Rimlingo] {initiatingPawn.LabelShort} initiated an interaction with {recipient.LabelShort}.");

            }

            // Attempt best language
            string chosenLanguage = LangUtils.DetermineBestLanguage(initiatingPawn, recipient);
            Log.Message($"[Rimlingo] \"{chosenLanguage}\" was chosen for the interaction.");
            if (string.IsNullOrEmpty(chosenLanguage))
            {
                LangUtils.GiveThought(initiatingPawn, recipient,
                    DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true));
                LangUtils.GiveThought(recipient, initiatingPawn,
                    DefDatabase<ThoughtDef>.GetNamed("LinguisticallyMisunderstood", true));
                //if no common language is found the interaction fails and original code is skipped. Will have to test to make sure it doesn't break.
                Log.Message($"[Rimlingo] Interaction failed between {initiatingPawn} and {recipient}");
                
                //return false to skip default code;
                __result = false;
                return false;
            }

            if (chosenLanguage == "Common")
            {
                Log.Message($"[Rimlingo] Interaction in Common took place between {initiatingPawn} and {recipient}");
                //return true to run default code.
                __result = true;
                return true; // No special effect
            }

            //otherwise give special effect.

            // Non-common => good thoughts & skill gain
            LangUtils.GiveThought(initiatingPawn, recipient,
                DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true));
            LangUtils.GiveThought(recipient, initiatingPawn,
                DefDatabase<ThoughtDef>.GetNamed("LinguisticallyUnderstood", true));

            int intA = LangUtils.GetPawnIntelligence(initiatingPawn);
            int intB = LangUtils.GetPawnIntelligence(recipient);
            if (intA > 0)
                LangUtils.AlterLanguageSkill(initiatingPawn, chosenLanguage, intA);
            if (intB > 0)
                LangUtils.AlterLanguageSkill(recipient, chosenLanguage, intB);

            if (initiatingPawn.def == recipient.def && initiatingPawn.Faction != recipient.Faction)
            {
                Log.Message($"[Rimlingo] {initiatingPawn.LabelShort} & {recipient.LabelShort} share species but different factions!");
            }

            __result = true;
            return false; //return false to run default code
        }
    }
}