using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Crypto;
using iText.Kernel;

namespace de.creinbold.SheetMusicBinder
{
    class InteractiveLayoutPrompt
    {
        public static bool AskUser(ConsoleKey yesKey = ConsoleKey.Y, ConsoleKey noKey = ConsoleKey.N)
        {
            for (var info = Console.ReadKey(true); ; info = Console.ReadKey(true))
            {
                if (info.Key == noKey) return false;
                if (info.Key == yesKey) return true;
            }
        }

        private LayoutedSheetPrinter _Printer;

        public InteractiveLayoutPrompt(string source)
        {
            _Printer = new LayoutedSheetPrinter(source);
        }

        public void Interact()
        {
            ShowPDF(_Printer.Source);
            _Printer.InsertEmptyPages = PromptInsertEmptyPages();
            var pages = PromptPageRange();
            var layout = PromptLayout(pages);
            while (layout == null)
            {
                Console.WriteLine("Layouting nicht möglich. Bitte mehr Stellen zum Blätter hinzufügen.");
                layout = PromptLayout(pages);
            }
            do
            {
                try
                {
                    var outString = _Printer.Print(pages, layout);
                    ShowPDF(outString);
                    break;
                }
                catch (Exception ex) when (ex is BadPasswordException || ex is PdfException)
                {
                    Console.WriteLine(String.Format("{0} kann nicht verarbeitet werden. Passwortschutz? Falls sich die PDF öffnen" +
                        "lässt, bitte Datei in eine neue PDF ausdrucken und alte Datei ersetzen.", _Printer.Source));
                    Console.WriteLine("Neuer Versuch? [Y/N]");
                }
            } while (AskUser());
        }

        private bool PromptInsertEmptyPages()
        {
            Console.Write("Leerseiten für doppelseitigen Druck einfügen? [Y/N] (voreingestellt auf N): ");
            bool result;
            string line = "";
            do
            {
                line = Console.ReadLine();
                if (String.IsNullOrEmpty(line)) return false;
            } while (!TryParseBool(line, out result));
            return result;
        }

        private bool TryParseBool(string line, out bool result)
        {
            line = line.ToLower();
            if (line.Equals("y") || line.Equals("j") || line.Equals("yes") || line.Equals("ja"))
            {
                result = true;
                return true;
            }
            if (line.Equals("n") || line.Equals("no") || line.Equals("nein"))
            {
                result = false;
                return true;
            }
            result = false;
            return false;
        }

        private List<int> PromptPageRange()
        {
            var firstPage = PromptNumber("Erste Seite", 1);
            var lastPage = PromptNumber("Letzte Seite", _Printer.NumberOfPages);
            firstPage = Math.Max(1, firstPage);
            lastPage = Math.Min(_Printer.NumberOfPages, lastPage);
            return Enumerable.Range(firstPage, lastPage - firstPage + 1).ToList();
        }

        private Layout PromptLayout(IList<int> pages)
        {
            var helpString = "Format zum Eingeben von Blätterpausen:\n" +
                             "  sX: Am Anfang der Seite kann über X Sekunden geblättert werden.\n" +
                             "  mX: In der Mitte der Seite kann über X Sekunden geblättert werden.\n" +
                             "  eX: Am Ende der Seite kann über X Sekunden geblättert werden.\n" +
                             "Wird X weggelassen, wird eine beliebig lange Zeit angenommen.\n" +
                             "Statt X kann auch X/Y geschrieben werden mit der Lesart: Bei einem Tempo von Y Schlägen die Minute sind X Schläge zum Blättern Zeit.\n" +
                             "Beispiele: m, s7e2, m7/120s";
            var pageSpace = PromptNumber("Zahl gleichzeitig offen aufliegender Seiten", 2);
            Console.WriteLine(helpString);
            pageSpace = Math.Max(2, pageSpace);
            var layouter = new Layouter(pageSpace);

            foreach(var page in pages)
            {
                while (true)
                {
                    decimal[] timings = null;
                    try
                    {
                        timings = PromptPage(page);
                    }
                    catch
                    {
                        Console.WriteLine("Ungültige Eingabe.\n" + helpString);
                        continue;
                    }
                    layouter.AddPage(timings[0], timings[1], timings[2]);
                    break;
                }
            }
            return layouter.GetOptimalLayouts().FirstOrDefault();
        }

        private decimal[] PromptPage(int pageNumber)
        {
            var result = new decimal[] { 0, 0, 0 };
            Console.Write(String.Format("Seite {0}: ", pageNumber));
            var line = Console.ReadLine().ToLower();
            Regex timeRegex = new Regex("^([1-9][0-9]*)(/[1-9][0-9]*)?$");
            while (!String.IsNullOrEmpty(line))
            {
                var symbol = line[0];
                var nextIdx = line.IndexOfAny(new char[] { 's', 'm', 'e' }, 1);
                if (nextIdx == -1) nextIdx = line.Length;
                var timeStr = line.Substring(1, nextIdx - 1).Trim();
                var time = Decimal.MaxValue / 3;
                if (!String.IsNullOrEmpty(timeStr))
                {
                    var match = timeRegex.Match(timeStr);
                    time = decimal.Parse(match.Groups[1].Value);
                    if (!String.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        time /= decimal.Parse(match.Groups[2].Value.Substring(1));
                    }
                    else
                    {
                        time /= 60;
                    }
                }
                if (symbol == 's') result[0] = time;
                else if (symbol == 'm') result[1] = time;
                else if (symbol == 'e') result[2] = time;
                else throw new FormatException("Substring did not start with s, m or e.");
                line = line.Substring(nextIdx);
            }
            return result;
        }

        private int PromptNumber(string text, int defaultValue)
        {
            Console.Write(String.Format("{0} (voreingestellt auf {1}): ", text, defaultValue));
            int number;
            string line = "";
            do
            {
                line = Console.ReadLine();
                if (String.IsNullOrEmpty(line)) return defaultValue;
            } while (!int.TryParse(line, out number));
            return number;
        }

        private void ShowPDF(string path)
        {
            System.Diagnostics.Process.Start(path);
        }
    }
}
