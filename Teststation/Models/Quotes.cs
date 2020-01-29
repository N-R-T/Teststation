using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public static class Quotes
    {
        private static QuoteViewModel currentQuote;
        private static DateTime lastRolled = DateTime.Now;
        private static readonly Dictionary<string, List<string>> quotes = new Dictionary<string, List<string>>
        {
            { "Gandalf", new List<string>{
                "Du bist ein Zauberer, " + Consts.quoteUserName + ".",
                "Hüte dich vor der dunklen Seite der Macht.",
                "Mein Schatz!",
                "Die überlegene Kunst des Krieges ist es, den Gegner zu überwältigen ohne zu kämpfen.",
            }},
            { "Angela Merkel",new List<string>{
                "Wir schaffen das.",
                "Aus großer Macht, folgt große Verantwortung!",
                "Houston, wir haben ein Problem!",
                "Heute ist nicht alle Tage, ich komm' wieder, keine Frage!",
                "Let's get crazy.",
                "Spieglein, Spieglein, an der Wand, wer ist die Schönste im ganzen Land?",
            }},
            { "Lobbyist zu Angela Merkel",new List<string>{                
                "Ich mache ihr ein Angebot, das sie nicht ablehnen kann.",
            }},
            { "Barrack Obama",new List<string>{
                "Yes, we can.",
                "Nobody's perfect.",
                "Let's get crazy.",
            }},
            { "Donald Trump",new List<string>{
                "Yippie-ya-yeah, Schweinebacke!",
                "Ich komme wieder!",
                "Hasta la vista, Baby!",
                "Ich bin der König der Welt!",
                "Palim-Palim.",
                "Houston, wir haben ein Problem!",
                "Heute ist nicht alle Tage, ich komm' wieder, keine Frage!",
                "Mein Schatz!",
                "Ich mache ihm ein Angebot, das er nicht ablehnen kann.",
                 "Spieglein, Spieglein, an der Wand, wer ist die Schönste im ganzen Land?",
                "Let's get crazy.",
            }},
            { "Albert Einstein",new List<string>{
                "Zwei Dinge sind unendlich: Das Universum und die menschliche Dummheit. Aber beim Universum bin ich mir nicht ganz sicher.",
                "Nobody's perfect.",
                "Das Leben wäre tragisch, wenn es nicht lustig wäre.",
            } },
            { "Darth Vader",new List<string>{
                Consts.quoteUserName + ". Ich bin dein Vater.",
                "Ich bin mit dir verwandt, Louis.",
                "Ich stehe deutlich über dir.",
                "I have the high ground.",
                "Möge die Macht mit dir sein.",
                "Aus großer Macht, folgt große Verantwortung!",
                "Houston, wir haben ein Problem!",
            }},
            { "Bernd das Brot",new List<string>{
                "Mist",
                "Möge die Macht mit dir sein.",
                "All I have are negative thoughts.",
                "Ich bin mit dir verwandt, Louis.",
                "Let's get crazy.",
            }},
            { "Martin Luther King",new List<string>{
                "I have a dream.",
                "Hüte dich vor der dunklen Seite der Macht.",
                "Aus großer Macht, folgt große Verantwortung!",
                "Wann stirbt ein Mann? Wenn er erschossen wird? Nein! Wenn er einen giftigen Pilz isst? Nein! Ein Mann stirbt erst, wenn er vergessen wird!",
            }},
            { "Stephen Hawking",new List<string>{
                "Let's get crazy.",
                "Zu fragen, was vor dem Beginn des Universums war, ist so sinnlos wie die Frage: Was ist nördlich vom Nordpol.",
                "Das Leben wäre tragisch, wenn es nicht lustig wäre.",
                "Der größte Feind des Wissens ist nicht Ignoranz, sondern die Illusion, wissend zu sein.",
                "Die größten menschlichen Errungenschaften sind durch Kommunikation zustande gekommen – die schlimmsten Fehler, weil nicht miteinander geredet wurde.",
                "Wann stirbt ein Mann? Wenn er erschossen wird? Nein! Wenn er einen giftigen Pilz isst? Nein! Ein Mann stirbt erst, wenn er vergessen wird!",
            }},
            { "Steve Jobs",new List<string>{
                "Let's get crazy.",
            }},
            { "Bill Gates",new List<string>{
                "640K sollte genug für jedermann sein.",
                "Nobody's perfect.",
                "Let's get crazy.",
            }},
            { "Forrest Gump",new List<string>{
                 "Das Leben ist wie eine Schachtel Pralinen, man weiß nie, was man bekommt.",
                 "Das Leben ist wie eine Schachtel Pralinen, man weiß nie, was man kriegt.",
            } },
            { "Micky Maus",new List<string>{
                "Ich bin der König der Welt!",
                "Aus großer Macht, folgt große Verantwortung!",
            }},
            { "Papa Schlumpf",new List<string>{
                "Wir brauchen mehr Wachstum.",
                "Das Leben ist wie eine Schachtel Pralinen, man weiß nie, was man bekommt.",
            }},
            { Consts.quoteUserName, new List<string>{
                "Ich bin " + Consts.quoteUserName + "!",
                "Eines Tages bin ich der König der Azubis!",
            }},
            { "Batman", new List<string>{
                "Ich bin Batman!",
                "Aus großer Macht, folgt große Verantwortung!",
                "Erscheine schwach wenn du stark bist, und stark wenn du schwach bist.",
            }},
            { "Sun Tzu", new List<string>{
                "Inmitten von Chaos, gibt es auch Gelegenheiten.",
                "Aus großer Macht, folgt große Verantwortung!",
                "Die überlegene Kunst des Krieges ist es, den Gegner zu überwältigen ohne zu kämpfen.",
                "Schwitze mehr während Friedenszeiten, blute weniger während Kriegszeiten.",
                "Erscheine schwach wenn du stark bist, und stark wenn du schwach bist.",
                "Wann stirbt ein Mann? Wenn er erschossen wird? Nein! Wenn er einen giftigen Pilz isst? Nein! Ein Mann stirbt erst, wenn er vergessen wird!",
                "Hüte dich vor der dunklen Seite der Macht.",
                "Merke dir, es gibt keine Fehler, nur neues Wissen.",
                "Es gibt keine Fehler, nur glückliche kleine Unfälle.",
            }},
            { "Bob Ross", new List<string>{
                "Es gibt keine Fehler, nur glückliche kleine Missgeschicke.",
                "I have a dream.",
                "Wir alle brauchen Freunde, sogar ein Baum.",
                "Merke dir, es gibt keine Fehler, nur neues Wissen.",
                "Let's get crazy.",
                "Let's build us a happy, little cloud that floats around the sky.",
            } },
        };
        private static void SetRandomQoute(string username)
        {
            Random random = new Random();
            var person = quotes.Select(x => x.Key).ToList()[random.Next(0, quotes.Count)];
            var quote = quotes[person][random.Next(0, quotes[person].Count)];

            currentQuote = new QuoteViewModel(quote, person, username);

            lastRolled = DateTime.Now;
        }
        public static QuoteViewModel GetCurrentQoute(string username)
        {
            if (currentQuote == null || DateTime.Now >= lastRolled.AddMinutes(30))
            {
                SetRandomQoute(username);
            }
            return currentQuote;
        }

        public static List<QuoteViewModel> GetAllQuotes(string username)
        {
            var allQuotes = new List<QuoteViewModel>();
            foreach (var person in quotes.Keys)
            {
                foreach (var quote in quotes[person])
                {
                    allQuotes.Add(new QuoteViewModel(quote, person, username));
                }
            }

            return allQuotes;
        }
    }

    public class QuoteViewModel
    {
        public string Quote { get; set; }
        public string Person { get; set; }       
        
        public QuoteViewModel(string quote, string person, string username)
        {
            Quote = quote.Replace(Consts.quoteUserName, username);
            Person = person.Replace(Consts.quoteUserName, username);
        }
    }
}
