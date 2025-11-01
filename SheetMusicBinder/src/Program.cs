using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace de.creinbold.SheetMusicBinder
{
    class Program
    {
        static IEnumerable<string> SelectSheetFilesByUser()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Datei(en) mit Noten auswählen.";
            dialog.Filter = "PDF Dateien (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK) return Enumerable.Empty<string>();
            return dialog.FileNames;
        }

        // Use STAThread since some components want to use dialogs.
        [STAThread]
        static void Main(string[] args)
        {
            foreach(var path in SelectSheetFilesByUser())
            {
                Console.WriteLine();
                new InteractiveLayoutPrompt(path).Interact();
                Console.WriteLine();
            }
        }
    }
}
