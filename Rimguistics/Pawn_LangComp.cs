using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace Rimguistics
{
    public class Pawn_LangComp : ThingComp
    {
        // Key = "German", "French", etc. Value = skill [0..100]
        public Dictionary<string, LangDef> Languages = new Dictionary<string, LangDef>();
        public int MaxLangs { get; private set; } = 1;

        public void SetMaxLangs(int maxLangs)
        {
            MaxLangs = maxLangs;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref Languages, "TTDG.languageSkills");
        }

        public float GetLanguageSkill(string languageDefName)
        {
            return Languages.TryGetValue(languageDefName, out LangDef lang) ? lang.Skill : 0f;
        }

        public void SetPreferredLanguage(string preferredLanguage)
        {
            if(!Languages.Any())
            {
                Log.Error($"[Rimguistics] Unable to set {preferredLanguage} as {parent.LabelShort}'s preferred language: No known languages.");
                return;
            }
            try
            {
                foreach (var lang in Languages)
                {
                    lang.Value.PreferredLanguage = false;
                }
                if( preferredLanguage != null )
                    Languages[preferredLanguage].PreferredLanguage = true;
            }
            catch (Exception e)
            {
                Log.Error($"[Rimguistics] Unable to set {preferredLanguage} as {parent.LabelShort}'s preferred language: " + e.Message);

            }
        }

        public bool SetLanguageSkill(string langName, float value)
        {
            var pawn = parent as Pawn;

            //Pawns can only learn a language if they have a language slot or already are learning it.
            if (Languages.Count < MaxLangs || Languages.Keys.Contains(langName))
            {

                if (!Languages.ContainsKey(langName))
                {
                    Languages.Add(langName, new LangDef(langName, value));
                    Languages[langName].SetSkill(value);
                }
                else
                {
                    Languages[langName].SetSkill(value);
                }

                if (Languages[langName].Skill < 0)
                {
                    LangUtils.RemoveLangFromPawn(pawn, langName);
                }
                return true;
            }
            else
            {
                Log.Warning(parent.LabelShort + " cannot learn " + langName);
                return false;
            }
        }

        public void ListKnownLangs()
        {
            Log.Message($"{parent.LabelShort}'s languages: ".Colorize(Color.cyan));
            foreach (var lang in Languages)
            {
                Log.Message($" - {lang.Value.ToString()}".Colorize(Color.cyan));
            }
        }
    }
}
