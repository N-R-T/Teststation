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
            new StringPair("é","@_e_@"),
            new StringPair("á","@_a_@"),
            new StringPair("í","@_i_@"),
            new StringPair("ó","@_o_@"),
            new StringPair("ú","@_u_@"),
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
