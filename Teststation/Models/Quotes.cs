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
            }},
            { "Angela Merkel",new List<string>{
                "Wir schaffen das.",
                "Aus großer Macht, folgt große Verantwortung!",
                "Houston, wir haben ein Problem!",
                "Heute ist nicht alle Tage, ich komm' wieder, keine Frage!",
            }},
            { "Lobbyist zu Angela Merkel",new List<string>{                
                "Ich mache ihr ein Angebot, das sie nicht ablehnen kann.",
            }},
            { "Barrack Obama",new List<string>{
                "Yes, we can.",
                "Nobody´s perfect.",
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
            }},
            { "Heidi Klum",new List<string>{
                 "Spieglein, Spieglein, an der Wand, wer ist die Schönste im ganzen Land?",
            }},
            { "Albert Einstein",new List<string>{
                "Zwei Dinge sind unendlich: Das Universum und die menschliche Dummheit. Aber beim Universum bin ich mir nicht ganz sicher.",
                "Nobody´s perfect.",
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
            }},
            { "Martin Luther King",new List<string>{
                "I have a dream.",
                "Hüte dich vor der dunklen Seite der Macht.",
                "Aus großer Macht, folgt große Verantwortung!",
            }},
            //{ "Martin Luther",new List<string>{ }},
            //{ "Stephen Hawking",new List<string>{ }},
            //{ "Steve Jobs",new List<string>{ }},
            //{ "Ash Ketchum",new List<string>{ }},
            //{ "Abraham Lincoln",new List<string>{ }},
            { "Bill Gates",new List<string>{
                "640K sollte genug für jedermann sein.",
                "Nobody´s perfect.",
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
            }},
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
