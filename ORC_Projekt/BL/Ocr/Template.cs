using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL.Ocr
{
    public class Template
    {
        public Template(string path, int value)
        {
            Path = path;
            Value = value;
            OriginalWithTemplateMatching = int.MaxValue;
            TemplateWithOriginalMatching = int.MaxValue;
            ResultMatching = int.MaxValue;
        }

        public string Path { get; private set; }
        public int Value { get; private set; }
        public double OriginalWithTemplateMatching { get; set; }
        public double TemplateWithOriginalMatching { get; set; }
        public double ResultMatching { get; set; }
    }
}
