using System.Reflection;
using HarmonyLib;
using Verse;

namespace Rimlingo
{
    [StaticConstructorOnStartup]
    public static class Rimlingo
    {
        static Rimlingo()
        {
            var harmony = new Harmony("CoolNether123.Rimlingo");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[Rimlingo] Harmony patches loaded successfully.");
        }
    }
}
