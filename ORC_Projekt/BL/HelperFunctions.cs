using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL
{
    class HelperFunctions
    {
        public static int GetGrayScaleFromColor(Color c)
        {
            return (int)((c.R + c.G + c.B) / 3);
        }
    }
}
