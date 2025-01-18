using System.Reflection;
using HarmonyLib;
using Verse;

namespace Rimguistics
{
    [StaticConstructorOnStartup]
    public static class Rimguistics
    {
        static Rimguistics()
        {
            var harmony = new Harmony("CoolNether123.Rimguistics");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[Rimguistics] Harmony patches loaded successfully.");
        }
    }
}
