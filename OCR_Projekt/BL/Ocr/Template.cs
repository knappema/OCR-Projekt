using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCR_Projekt.BL.Ocr
{
    public class Template
    {
        public Template(string path, int value)
        {
            Path = path;
            Value = value;
            OriginalOnTemplateMatching = int.MaxValue;
            TemplateOnOriginalMatching = int.MaxValue;
            MatchingAverage = double.MaxValue;
            MatchingDistanceAverage = double.MaxValue;
        }

        public string Path { get; private set; }
        public int Value { get; private set; }
        public double OriginalOnTemplateMatching { get; set; }
        public double TemplateOnOriginalMatching { get; set; }
        public double MatchingAverage { get; set; }
        public double MatchingDistanceAverage { get; set; }
    }
}
