using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comperssability
{
   public class FullTextInfo
    {
        public string Name { get; set; }
        public string InvertedName { get; set; }
        public long ContentSize { get; set; }
        public long InvertedContentSize { get; set; }
        public long ContentCompressedSize { get; set; }
        public long InvertedContentCompressedSize { get; set; }
        public double ContentCompressibilityCoeficient { get; set; }
        public double InvertedContentCompressibilityCoeficient { get; set; }
        public double Differnce { get; set; }


    }
}
