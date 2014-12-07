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
            Matching = int.MaxValue;
        }

        public string Path { get; private set; }
        public int Value { get; private set; }
        public double Matching { get; set; }
    }
}
