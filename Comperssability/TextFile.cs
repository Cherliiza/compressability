using LZStringCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comperssability
{
    public class TextFile
    {
        public string Name { get; }
        public string Content { get; }

        public long Size { get; }
        public string CompressedContent { get; private set; }
        public long CompressedContentSize { get; private set; }
        public double CompressibilityCoeficient { get; private set; }
        public string Path { get; }

        public TextFile(string name, string content, long size, string path)
        {
            Name = name;
            Content = content;
            Size = size;
            Path = path;
        }

        public void Compress()
        {
            CompressedContent = LZString.CompressToUTF16(Content);
            GetCompressedSize();
        }

        public string GetNameWithoutSufix()
        {
            var result = Name.Remove(Name.LastIndexOf("_"), Name.LastIndexOf(".txt"));
            return result;
        }

        private void GetCompressedSize()
        {
            string path = $"{Path}\\{ChangeTextFileNameWithSufix(Name, "_compressed")}";
            File.WriteAllText(path, CompressedContent);
            var file = new FileInfo(path);
            CompressedContentSize = file.Length;
            CalculateCompressibilityCoeficient();
            file.Delete();
        }
        private void CalculateCompressibilityCoeficient()
        {
            CompressibilityCoeficient = ((double)(Size - CompressedContentSize)) / ((double)Size);
        }
        private string ChangeTextFileNameWithSufix(string textFileName, string sufix)
        {
            if (textFileName.EndsWith(".txt"))
            {
                textFileName = textFileName.Insert(textFileName.LastIndexOf(".txt"), sufix);
                return textFileName;
            }

            return $"{textFileName}{sufix}";
        }

    }
}
