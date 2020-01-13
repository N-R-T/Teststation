using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class StringReplacer
    {
        private class StringPair
        {
            public string ForbiddenString { get; set; }
            public string ReplaceString { get; set; }

            public StringPair(string forbidden, string replace)
            {
                ForbiddenString = forbidden;
                ReplaceString = replace;
            }
        }

        private static readonly List<StringPair> stringPairs = new List<StringPair> {
            new StringPair("á","@_aApostroph_@"),
            new StringPair("é","@_eApostroph_@"),            
            new StringPair("í","@_iApostroph_@"),
            new StringPair("ó","@_oApostroph_@"),
            new StringPair("ú","@_uApostroph_@"),
            new StringPair("ý","@_yApostroph_@"),
            new StringPair("Á","@_AApostroph_@"),
            new StringPair("É","@_EApostroph_@"),
            new StringPair("Í","@_IApostroph_@"),
            new StringPair("Ó","@_OApostroph_@"),
            new StringPair("Ú","@_UApostroph_@"),
            new StringPair("Ý","@_YApostroph_@"),
            new StringPair("´","@_Apostroph_@"),
            new StringPair("â","@_aZirkumflex_@"),
            new StringPair("ê","@_eZirkumflex_@"),
            new StringPair("î","@_iZirkumflex_@"),
            new StringPair("ô","@_oZirkumflex_@"),
            new StringPair("û","@_uZirkumflex_@"),
            new StringPair("Â","@_AZirkumflex_@"),
            new StringPair("Ê","@_EZirkumflex_@"),
            new StringPair("Î","@_IZirkumflex_@"),
            new StringPair("Ô","@_OZirkumflex_@"),
            new StringPair("Û","@_UZirkumflex_@"),
            new StringPair("^","@_Zirkumflex_@"),
            new StringPair("à","@_aGravis_@"),
            new StringPair("è","@_eGravis_@"),
            new StringPair("ì","@_iGravis_@"),
            new StringPair("ò","@_oGravis_@"),
            new StringPair("ù","@_uGravis_@"),
            new StringPair("À","@_AGravis_@"),
            new StringPair("È","@_EGravis_@"),
            new StringPair("Ì","@_IGravis_@"),
            new StringPair("Ò","@_OGravis_@"),
            new StringPair("Ù","@_UGravis_@"),
            new StringPair("`","@_Gravis_@"),
            new StringPair("?","@_Fragezeichen_@"),
            new StringPair("!","@_Ausrufezeichen_@"),
            new StringPair("ß","@_SZ_@"),
            new StringPair("<","@_KleinerAls_@"),
            new StringPair(">","@_GrosserAls_@"),
            new StringPair(",","@_Komma_@"),
            new StringPair(";","@_Semikolon_@"),
            new StringPair(":","@_Doppelpunkt_@"),
            new StringPair("*","@_Sternchen_@"),
            new StringPair("~","@_Tilde_@"),
            new StringPair("(","@_KlammerAuf_@"),
            new StringPair(")","@_KlammerZu_@"),
            new StringPair("{","@_GeschwungeneKlammerAuf_@"),
            new StringPair("}","@_GeschwungeneKlammerZu_@"),
            new StringPair("[","@_EckigeKlammerAuf_@"),
            new StringPair("]","@_EckigeKlammerZu_@"),
            new StringPair("§","@_Paragraph_@"),
            new StringPair("$","@_Dollar_@"),
            new StringPair("Æ","@_Altesae_@"),
        };

        public static string ConvertToDatabase(string input)
        {
            foreach (var pair in stringPairs)
            {
                input = input.Replace(pair.ForbiddenString, pair.ReplaceString);
            }
            return input;
        }

        public static string ConvertFromDatabase(string input)
        {
            foreach (var pair in stringPairs)
            {
                input = input.Replace(pair.ReplaceString, pair.ForbiddenString);
            }
            return input;
        }
    }
}
