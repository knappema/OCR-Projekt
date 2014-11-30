using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ORC_Projekt.BL.Ocr
{
    class OcrHelper
    {
        public static Bitmap RemoveRed(Bitmap input)
        {
            //top
            for (int x = 0; x < input.Width; x++) 
            {
                for (int y = 0; y < 10; y++)
                {
                    var c = input.GetPixel(x, 0);
                    if (c == Color.Red)
                    {
                        input.SetPixel(x, 0, Color.White);
                    }
                }

            }

            //botom
            for (int x = 0; x < input.Width; x++)
            {
                if (input.GetPixel(x, input.Height - 1) == Color.Red)
                {
                    input.SetPixel(x, input.Height - 1, Color.White);
                }
            }

            //left
            for (int y = 0; y < input.Height; y++)
            {
                if (input.GetPixel(0, y) == Color.Red)
                {
                    input.SetPixel(0, y, Color.White);
                }
            }

            //right
            for (int y = 0; y < input.Height; y++)
            {
                if (input.GetPixel(input.Width - 1, y) == Color.Red)
                {
                    input.SetPixel(input.Width - 1, y, Color.White);
                }
            }

            return input;
        }
    }
}
