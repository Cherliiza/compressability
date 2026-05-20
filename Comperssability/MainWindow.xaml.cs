using LZStringCSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Comperssability
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string pathToOriginal = @"..\..\..\original texts";
        const string pathToInvertedByChars = @"..\..\..\inverted texts by chars";
        const string pathToInvertedByWords = @"..\..\..\inverted texts by words";
        const string pathToInvertedBySentences = @"..\..\..\inverted texts by sentences";
        const string pathToDivided1 = @"..\..\..\divided texts half 1";
        const string pathToDivided2 = @"..\..\..\divided texts half 2";

        public ObservableCollection<FullTextInfo> IbcTextFiles { get; set; } = new ObservableCollection<FullTextInfo>();
        public ObservableCollection<FullTextInfo> IbwTextLists { get; set; } = new ObservableCollection<FullTextInfo>();
        public ObservableCollection<FullTextInfo> IbsTextFiles { get; set; } = new ObservableCollection<FullTextInfo>();
        public ObservableCollection<DividedText> DividedTextFiles { get; set; } = new ObservableCollection<DividedText>();
        public ObservableCollection<ZScore> Zscores { get; set; } = new ObservableCollection<ZScore>();


        public ZScore ZScore = new ZScore();
        public MainWindow()
        {
            InitializeComponent();
            IBCTextFilesDataGrid.ItemsSource = IbcTextFiles;
            IBWTextFilesDataGrid.ItemsSource = IbwTextLists;
            IBSTextFilesDataGrid.ItemsSource = IbsTextFiles;
            DividedTextFilesDataGrid.ItemsSource = DividedTextFiles;

            ZScoresGrid.ItemsSource = Zscores;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           /* if (csRadio.IsChecked.Value)
            {
                pathToOriginal = @"..\..\..\initial shuffled texts by chars";
            }
            else if (wsRadio.IsChecked.Value)
            {
                pathToOriginal = @"..\..\..\initial shuffled texts by words";

            }
            else if (ssRadio.IsChecked.Value)
            {
                pathToOriginal = @"..\..\..\initial shuffled texts by sentences";

            }*/

            GetAllTexts();
        }

        private async void GetAllTexts()
        {
            var originalTextsTask = Task.Run(() => GetTextFiles(pathToOriginal));
            var ibcTextsTask = Task.Run(() => GetTextFiles(pathToInvertedByChars));
            var ibwTextsTask = Task.Run(() => GetTextFiles(pathToInvertedByWords));
            var ibsTextsTask = Task.Run(() => GetTextFiles(pathToInvertedBySentences));
            var dividedTexts1Task = Task.Run(() => GetTextFiles(pathToDivided1));
            var dividedTexts2Task = Task.Run(() => GetTextFiles(pathToDivided2));


            await Task.WhenAll(originalTextsTask, ibcTextsTask, ibwTextsTask, ibsTextsTask);

            var originalTexts = originalTextsTask.Result.OrderBy(x => x.Name).ToList();
            var ibcTexts = ibcTextsTask.Result.OrderBy(x => x.Name).ToList();
            var ibwTexts = ibwTextsTask.Result.OrderBy(x => x.Name).ToList();
            var ibsTexts = ibsTextsTask.Result.OrderBy(x => x.Name).ToList();
            var dividedTexts1 = dividedTexts1Task.Result.OrderBy(x => x.Name).ToList();
            var dividedTexts2 = dividedTexts2Task.Result.OrderBy(x => x.Name).ToList();

            Parallel.ForEach(originalTexts, original =>
           {
               original.Compress();
           });

            var taskIBC = Task.Run(() => GetFullTextsInfo(originalTexts, ibcTexts));
            var taskIBW = Task.Run(() => GetFullTextsInfo(originalTexts, ibwTexts));
            var taskIBS = Task.Run(() => GetFullTextsInfo(originalTexts, ibsTexts));
            var taskDivided = Task.Run(() => GetDividedTextsInfo(dividedTexts1, dividedTexts2)); 
            await Task.WhenAll(taskIBC, taskIBW, taskIBS, taskDivided);

            taskIBC.Result.ForEach(x => IbcTextFiles.Add(x));
            taskIBW.Result.ForEach(x => IbwTextLists.Add(x));
            taskIBS.Result.ForEach(x => IbsTextFiles.Add(x));
            taskDivided.Result.ForEach(x => DividedTextFiles.Add(x));

            ZScore.ZScoreIBC = GetZScore(IbcTextFiles.Select(x => x.Differnce).ToList());
            ZScore.ZScoreIBW = GetZScore(IbwTextLists.Select(x => x.Differnce).ToList());
            ZScore.ZScoreIBS = GetZScore(IbsTextFiles.Select(x => x.Differnce).ToList());
            ZScore.ZScoreDivided = GetZScore(DividedTextFiles.Select(x => x.Difference).ToList());
            Zscores.Add(ZScore);


        }

        private async Task<List<FullTextInfo>> GetFullTextsInfo(List<TextFile> originalsTexts, List<TextFile> invertedTexts)
        {
            var result = new List<FullTextInfo>();

            await Task.Run(() => Parallel.ForEach(invertedTexts, inverted =>
            {
                inverted.Compress();
            }));


            var invertedAndOriginal = originalsTexts.Zip(invertedTexts, (o, i) => new { Original = o, Inverted = i });

            foreach (var io in invertedAndOriginal)
            {
                result.Add(new FullTextInfo
                {
                    Name = io.Original.Name,
                    InvertedName = io.Inverted.Name,
                    ContentSize = io.Original.Size,
                    ContentCompressedSize = io.Original.CompressedContentSize,
                    InvertedContentSize = io.Inverted.Size,
                    InvertedContentCompressedSize = io.Inverted.CompressedContentSize,
                    ContentCompressibilityCoeficient = io.Original.CompressibilityCoeficient,
                    InvertedContentCompressibilityCoeficient = io.Inverted.CompressibilityCoeficient,
                    Differnce = io.Original.CompressibilityCoeficient - io.Inverted.CompressibilityCoeficient
                }
              );
            }
            return result;
        }

        private async Task<List<DividedText>> GetDividedTextsInfo(List<TextFile> dividedTexts1, List<TextFile> dividedTexts2)
        {
            var result = new List<DividedText>();

            var task1 = Task.Run(() => Parallel.ForEach(dividedTexts1, divided =>
            {
                divided.Compress();
            }));

            var task2 = Task.Run(() => Parallel.ForEach(dividedTexts2, divided =>
            {
                divided.Compress();
            }));

            await Task.WhenAll(task1, task2);

            var dividedParts = dividedTexts1.Zip(dividedTexts2, (f, s) => new { First = f, Second = s });

            foreach (var fs in dividedParts)
            {
                result.Add(new DividedText
                {
                    NameFirstPart = fs.First.Name,
                    NameSecondPart = fs.Second.Name,
                    ContentFirstPart = fs.First.Content,
                    ContentSecondPart = fs.Second.Content,
                    SizeFirstPart = fs.First.Size,
                    SizeSecondPart = fs.Second.Size,
                    CompressedContentSizeFirstPart = fs.First.CompressedContentSize,
                    CompressedContentSizeSecondPart = fs.Second.CompressedContentSize,
                    CompressibilityCoeficientFirstPart = fs.First.CompressibilityCoeficient,
                    CompressibilityCoeficientSecondPart = fs.Second.CompressibilityCoeficient,
                    Difference = fs.First.CompressibilityCoeficient - fs.Second.CompressibilityCoeficient
                }
              );
            }

            return result;
        }


        private static List<TextFile> GetTextFiles(string path)
        {
            var textFilesCollection = new List<TextFile>();

            Parallel.ForEach(Directory.GetFiles(path, "*.txt"), file =>
            {
                var name = System.IO.Path.GetFileName(file);
                var content = File.ReadAllText(file);
                var length = new FileInfo(file).Length;
                textFilesCollection.Add(new TextFile
                (
                    name,
                   content,
                   length,
                   path
                ));
            });
            return textFilesCollection;
        }

        private static double GetZScore(List<double> Differences)
        {
            int i = 1;
            var sorted = Differences.OrderBy(x => Math.Abs(x)).Select(x => x).Where(x => Math.Abs(x) >= 0.0001);
            var ranks = sorted.Select(x => Math.Sign(x) * i++).ToList();

            var W = ranks.Sum();
            var roCeoff = GetRoCoeff(ranks.Count);

            return W / roCeoff;
        }

        private static double GetRoCoeff(int amount)
        {
            return Math.Sqrt(((amount * (amount + 1) * (2 * amount + 1)) / 6));
        }

        /*        private static string GetNameWithousSufix(string original)
                {
                    return original.Remove(original.LastIndexOf('_cl'));
                }*/
    }

    public class ZScore
    {
        public double ZScoreIBC { get; set; }
        public double ZScoreIBW { get; set; }
        public double ZScoreIBS { get; set; }
        public double ZScoreDivided { get; set; }

    }

    public class DividedText
    {
        public string NameFirstPart { get;  set; }
        public string NameSecondPart { get; set; }

        public string ContentFirstPart { get;  set; }
        public string ContentSecondPart { get;  set; }


        public long SizeFirstPart { get; set; }
        public long SizeSecondPart { get;  set; }

        public long CompressedContentSizeFirstPart { get;  set; }
        public long CompressedContentSizeSecondPart { get;  set; }

        public double CompressibilityCoeficientFirstPart { get;  set; }
        public double CompressibilityCoeficientSecondPart { get;  set; }
        public double Difference { get; set; }

    }
}
