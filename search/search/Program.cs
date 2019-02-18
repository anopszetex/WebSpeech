using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Speech.Recognition;
using System.IO;

namespace search
{
    class Program
    {
        private static SpeechRecognitionEngine engine;
        private string[] SplitGrammar(string[] arr) {
            List<string> g = new List<string>();
            for (int i = 0; i < arr.Length; i++) {
                string[] words = arr[i].Split(' '); // dividir string
                for (int j = 0; j < words.Length; j++) {
                    if (!g.Contains(words[j])) {
                        g.Add(words[j]);
                    }
                }
            }
            return g.ToArray();
        }

        static void Main(string[] args) {
            try
            {

                engine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-BR"));
                engine.SetInputToDefaultAudioDevice(); //definindo saída do audio.

                Choices webSearch = new Choices();//Carregando palavras do arquivo txt.
                StreamReader srWords = new StreamReader(@"words.txt", Encoding.UTF8);
                while (srWords.Peek() >= 0) {
                    try
                    {
                        webSearch.Add(srWords.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro:a " + ex);
                    }
                }
                srWords.Close();

                //Grammar de busca
                GrammarBuilder gbWebSearch = new GrammarBuilder();
                gbWebSearch.Append(new Choices("pesquisar", "buscar"));
                gbWebSearch.Append(webSearch);
                gbWebSearch.Append(new Choices("no youtube", "no google"));
                Grammar gWebSearch = new Grammar(gbWebSearch);
                gWebSearch.Name = "Search";

                List<Grammar> grammars = new List<Grammar>();
                grammars.Add(gWebSearch);

                //carregamento paralelo.
                ParallelOptions op = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
                Parallel.For(0, grammars.Count, op, i =>
                {
                    engine.LoadGrammar(grammars[i]);
                });

                string[] phrases = { "olá" };
                engine.LoadGrammar(new Grammar(new Choices(phrases)));
                engine.LoadGrammarCompleted += loaded;
                engine.SpeechRecognized += rec; // evento do reconhecimento
                engine.RecognizeAsync(RecognizeMode.Multiple); // inicia o reconhecimento

                Console.ReadKey();
            }
            catch (Exception ex) { Console.WriteLine("Erro: " + ex); }
        }



        private static void rec(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;

            if (e.Result.Text == "olá")
                Console.WriteLine("olá " + Environment.UserName);

            switch (e.Result.Grammar.Name)
            {
                case "Search":
                    string[] parts = speech.Split(' ');
                    string text = "";
                    for (int k = 0; k < parts.Length; k++)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(parts[k], "[A-Z]"))
                        {
                            text += parts[k] + " ";
                        }
                    }
                    if (speech.ToLower().EndsWith("google"))
                    {
                        if (speech.Contains(" "))
                            speech = speech.Replace(" ", "+");
                        Console.WriteLine("certo pesquisando " + text + " no google");
                        System.Diagnostics.Process.Start("www.google.com/search?q=" + text);
                    }
                    break;
            }
        }

        private static void loaded(object s, LoadGrammarCompletedEventArgs e)
        {
            int grammar = 0;
            grammar++;
        }

    }

}
