using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ORC_Projekt.BL
{
    public class OcrManager
    {
        Bitmap _originalImage;

        public OcrManager(string fileName)
        {
            _originalImage = new Bitmap(fileName);
        }
    }
}
