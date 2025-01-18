using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimlingo
{
    

    [DefOf]
    public static class LangNamerDefOf 
    {
        public static RulePackDef ByzLangNamer;
        public static RulePackDef DirtLangNamer;
        public static RulePackDef EnglishLangNamer;
        public static RulePackDef GalicianLangNamer;
        public static RulePackDef ImpidLangNamer;
        public static RulePackDef NeanderthalLangNamer;
        public static RulePackDef PigLangNamer;
        public static RulePackDef WasterLangNamer;

        public static RulePackDef RandomLangNamer()
        {
            switch (Rand.Range(0, 7))
            {
                case 0:
                    return ByzLangNamer;
                case 1:
                    return DirtLangNamer;
                case 2:
                    return EnglishLangNamer;
                case 3:
                    return GalicianLangNamer;
                case 4:
                    return ImpidLangNamer;
                case 5:
                    return NeanderthalLangNamer;
                case 6:
                    return PigLangNamer;
                case 7:
                    return WasterLangNamer;

                default: 
                    return EnglishLangNamer;
            }
        }
    }

}
