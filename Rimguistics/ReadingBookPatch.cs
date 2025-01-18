using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Rimguistics
{
    [HarmonyPatch]
    public static class ReadingBookPatch
    {
        // If this is false, we skip patching entirely.
        static bool Prepare()
        {
            return TargetMethod() != null;
        }

        static MethodBase TargetMethod()
        {
            var rimWriterAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "RimWriter");
            if (rimWriterAssembly == null)
                return null;

            var bookReadingType = rimWriterAssembly.GetType("RimWriter.BookReadingUtility");
            if (bookReadingType == null)
                return null;

            return AccessTools.Method(bookReadingType, "FinishReading", new[] { typeof(Pawn), typeof(Thing) });
        }

        [HarmonyPostfix]
        public static void Postfix_FinishReading(Pawn reader, Thing book)
        {
            if (book.def.defName.Contains("LanguageBook"))
            {
                string languageDef = null;
                if (book.def.defName.Contains("German")) languageDef = "German";
                if (book.def.defName.Contains("French")) languageDef = "French";
                if (book.def.defName.Contains("Pigese")) languageDef = "Pigese";
                // etc.

                if (!string.IsNullOrEmpty(languageDef))
                {
                    int intellect = LangUtils.GetPawnIntelligence(reader);
                    if (intellect > 0)
                    {
                        float gain = intellect * 2f;
                        LangUtils.AlterLanguageSkill(reader, languageDef, gain);
                        Log.Message($"[Rimguistics] {reader.LabelShort} improved {languageDef} by {gain} via reading {book.def.label}!");
                    }
                }
            }
        }
    }
}