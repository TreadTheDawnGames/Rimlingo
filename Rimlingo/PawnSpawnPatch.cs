using HarmonyLib;
using Rimlingo;
using Verse;

[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
public static class PawnSpawnPatch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, bool respawningAfterLoad)
    {
        // Check if the pawn is humanlike
        if (__instance.RaceProps.Humanlike)
        {
            if (__instance.TryGetComp<CompPawnLanguages>() == null)
            {
                // Force-add the comp
                var comp = new CompPawnLanguages();
                comp.parent = __instance;
                __instance.AllComps.Add(comp);
            }
        }
    }
}
