using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextWorker
{
    /// <summary>
    /// This program cleans texts from not number-alphabetical signs, multi-spaces, new lines and leaves only '.' as a separator.
    /// Then it inverts each text by characters, by words and by sentences.
    /// Initial texts folder  - your initial texts.
    /// Original texts folder - cleaned texts.
    /// Inverted by chars/words/sentences folder - inverted texts.
    /// Text will be created in certain folders as you see below.
    /// For make program working, please add your .txt files into "initial texts" folder after first time execution.
    /// </summary>
    class Program
    {
        static string pathToInitial = @"..\..\..\initial texts";
        const string pathToOriginal = @"..\..\..\original texts";
        const string pathToDivided1 = @"..\..\..\divided texts half 1";
        const string pathToDivided2 = @"..\..\..\divided texts half 2";
        const string pathToInvertedByChars = @"..\..\..\inverted texts by chars";
        const string pathToInvertedByWords = @"..\..\..\inverted texts by words";
        const string pathToInvertedBySentences = @"..\..\..\inverted texts by sentences";
        const string pathToShuffledIBC = @"..\..\..\initial shuffled texts by chars";
        const string pathToShuffledIBW = @"..\..\..\initial shuffled texts by words";
        const string pathToShuffledIBS = @"..\..\..\initial shuffled texts by sentences";

        static List<Text> initialTexts = new List<Text>();
        static List<Text> originalTexts = new List<Text>();
        static List<Text> dividedTexts1 = new List<Text>();
        static List<Text> dividedTexts2 = new List<Text>();

        static void Main(string[] args)
        {
            Console.WriteLine("Which initial texts take ?\npress 1 if original\npress 2 if shuffle by chars\npress 3 if shuffled by words\npress 4 if shuffled by sentences");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice!=1)
            {

                if (choice == 2)
                {
                    pathToInitial = @"..\..\..\initial shuffled texts by chars";
                }
                else if (choice == 3)
                {
                    pathToInitial = @"..\..\..\initial shuffled texts by words";

                }
                else if (choice == 4)
                {
                    pathToInitial = @"..\..\..\initial shuffled texts by sentences";

                }



                List<DirectoryInfo> diList = new List<DirectoryInfo>
                {
                 new DirectoryInfo(pathToOriginal),
                 new DirectoryInfo(pathToInvertedByChars),
                 new DirectoryInfo(pathToInvertedByWords),
                 new DirectoryInfo(pathToInvertedBySentences),
                 new DirectoryInfo(pathToDivided1),
                 new DirectoryInfo(pathToDivided2)
                };

                foreach (var di in diList)
                {
                    foreach (FileInfo file in di.EnumerateFiles())
                    {
                        file.Delete();
                    }
                }
            }
            Console.WriteLine("Program has started! Please wait job to be done...");
            CreateDirectories();

            initialTexts = GetTextFiles(pathToInitial);
            CleanTexts(initialTexts);
            ProcessTexts();

            Console.WriteLine("Program has finished!");
            Console.WriteLine("Press any button to exit...");
            Console.ReadKey();
        }

        static void CleanTexts(List<Text> texts)
        {
            //clean from signs,but not from '?!.' and also new lines, multi spaces.
            Regex regex1 = new Regex(@"[^\s\w?!.]|[\r\n]|[ ]{2,}");
            //clean from ?! to .
            Regex regex2 = new Regex(@"[?!]");
            //clean from .. or ... and so on
            Regex regex3 = new Regex(@"[.]{2,}");


            Parallel.ForEach(texts, text =>
            {
                string result = regex1.Replace(text.Content, string.Empty);
                result = regex2.Replace(result, ".");
                result = regex3.Replace(result, string.Empty);

                originalTexts.Add(
                    new Text
                    {
                        Name = ChangeTextFileNameWithSufix(text.Name, "_cl"),
                        Content = result.ToLower()
                    });
            });
            WriteInTheFolderWithName(pathToOriginal, originalTexts);

            Console.WriteLine("Cleaning is done!");
        }

        static void ProcessTexts()
        {
            var invertedByChars = new List<Text>();
            var invertedByWords = new List<Text>();
            var invertedBySentences = new List<Text>();
            var dividedTexts1 = new List<Text>();
            var dividedTexts2 = new List<Text>();


            Parallel.ForEach(originalTexts, file =>
            {
                invertedByChars.Add(
                    new Text
                    {
                        Name = ChangeTextFileNameWithSufix(file.Name, "_ibc"),
                        Content = InvertByChars(file.Content)
                    });

                invertedByWords.Add(
                  new Text
                  {
                      Name = ChangeTextFileNameWithSufix(file.Name, "_ibw"),
                      Content = InvertByWords(file.Content)
                  });

                invertedBySentences.Add(
                  new Text
                  {
                      Name = ChangeTextFileNameWithSufix(file.Name, "_ibs"),
                      Content = InvertBySentences(file.Content)
                  });

                var texts = DivideTexts(file.Content);

                dividedTexts1.Add(
                    new Text
                    {
                        Name = ChangeTextFileNameWithSufix(file.Name, "dvd"),
                        Content = texts.Item1
                    });

                dividedTexts2.Add(
                   new Text
                   {
                       Name = ChangeTextFileNameWithSufix(file.Name, "dvd"),
                       Content = texts.Item2
                   });
            });

            var task1 = Task.Run(() => WriteInTheFolderWithName(pathToInvertedByChars, invertedByChars));
            var task2 = Task.Run(() => WriteInTheFolderWithName(pathToInvertedByWords, invertedByWords));
            var task3 = Task.Run(() => WriteInTheFolderWithName(pathToInvertedBySentences, invertedBySentences));
            var task4 = Task.Run(() => WriteInTheFolderWithName(pathToDivided1, dividedTexts1));
            var task5 = Task.Run(() => WriteInTheFolderWithName(pathToDivided2, dividedTexts2));


            Task.WaitAll(task1, task2, task3, task4, task5);

            Console.WriteLine("Inverting is done!");
        }

        static string InvertByChars(string original)
        {
            return new string(original.Reverse().ToArray());
        }

        static string InvertByWords(string original)
        {
            return string.Join(" ", original.Split(' ').Reverse());
        }

        static string InvertBySentences(string original)
        {
            //The only one sign in the text is dot
            return string.Join(".", original.Split('.').Reverse());
        }

        /*       static (string, string) DivideTexts(string original)
               {
                   var a =  original.Split('.');
                   var firstHalfCount = a.Count() - a.Count() / 2;
                   string firstText = string.Join(".", a.Take(firstHalfCount));
                   string secondText = string.Join(".", a.Skip(firstHalfCount));

                   return (firstText, secondText);
               }
       */

        static (string, string) DivideTexts(string original)
        {
            var a = original.ToCharArray();
            var firstHalfCount = a.Count() - a.Count() / 2;
            string firstText = new string(a.Take(firstHalfCount).ToArray());
            string secondText = new string(a.Skip(firstHalfCount).ToArray());

            return (firstText, secondText);
        }

        private static List<Text> GetTextFiles(string path)
        {
            var textFilesCollection = new List<Text>();

            Parallel.ForEach(Directory.GetFiles(path, "*.txt"), file =>
            {
                var name = Path.GetFileName(file);
                var content = File.ReadAllText(file);
                textFilesCollection.Add(new Text
                {
                    Name = name,
                    Content = content
                });
            });
            return textFilesCollection;
        }
        private static string ChangeTextFileNameWithSufix(string textFileName, string sufix)
        {
            if (textFileName.EndsWith(".txt"))
            {
                textFileName = textFileName.Insert(textFileName.LastIndexOf(".txt"), sufix);
                return textFileName;
            }

            return $"{textFileName}{sufix}";
        }

        private static void WriteInTheFolderWithName(string folderName, List<Text> textFiles)
        {
            Parallel.ForEach(textFiles, textFile =>
                   {
                       string path = $"{folderName}\\{textFile.Name}";
                       File.WriteAllText(path, textFile.Content);
                   });
        }

        private static void CreateDirectories()
        {
            Directory.CreateDirectory(pathToInitial);
            Directory.CreateDirectory(pathToOriginal);
            Directory.CreateDirectory(pathToInvertedByChars);
            Directory.CreateDirectory(pathToInvertedByWords);
            Directory.CreateDirectory(pathToInvertedBySentences);
            Directory.CreateDirectory(pathToDivided1);
            Directory.CreateDirectory(pathToDivided2);

        }

        struct Text
        {
            public string Content { get; set; }
            public string Name { get; set; }
        }
    }
}
