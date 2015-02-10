using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OCR_Projekt.BL.PreProcessing
{
    class ThinningWrapper
    {
        /// <summary>
        /// Thin the bitmap. Ideally characters are only 1px wide after this step
        /// </summary>
        public static Bitmap Thin(Bitmap img)
        {
            img = (Bitmap)img.Clone();
            bool[][] t = Image2Bool(img);
            t = ZhangSuenThinning(t);
            return Bool2Image(t);
        }

        /// <summary>
        /// Convert binary image into bool array. For faster computation
        /// </summary>
        private static bool[][] Image2Bool(Bitmap img)
        {
            int grayScale;

            Bitmap bmp = (Bitmap)img.Clone();
            bool[][] s = new bool[bmp.Height][];
            for (int y = 0; y < bmp.Height; y++)
            {
                s[y] = new bool[bmp.Width];
                for (int x = 0; x < bmp.Width; x++)
                {
                    grayScale = HelperFunctions.GetGrayScaleFromColor(bmp.GetPixel(x, y));
                    s[y][x] = (grayScale == 0);
                }
            }
            return s;
        }

        /// <summary>
        /// Convert bool array back into bitmap
        /// </summary>
        private static Bitmap Bool2Image(bool[][] s)
        {
            Bitmap bmp = new Bitmap(s[0].Length, s.Length);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (s[y][x])
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// Thin the image represented as a bool array using the Zhang Suen Algorithm (http://rosettacode.org/wiki/Zhang-Suen_thinning_algorithm)
        /// </summary>
        private static bool[][] ZhangSuenThinning(bool[][] s)
        {
            bool[][] temp = ArrayClone(s);
            int count = 0;
            do
            {
                count = step(1, temp, s);
                temp = ArrayClone(s);
                count += step(2, temp, s);
                temp = ArrayClone(s);
            }
            while (count > 0); //as long as some pixel changed

            return s;
        }

        /// <summary>
        /// Perform single step (either 1 or 2 ) return the amount of changed pixels
        /// </summary>
        private static int step(int stepNo, bool[][] temp, bool[][] s)
        {
            int count = 0;

            for (int a = 1; a < temp.Length - 1; a++)
            {
                for (int b = 1; b < temp[0].Length - 1; b++)
                {
                    if (SuenThinningAlg(a, b, temp, stepNo == 2))
                    {
                        if (s[a][b]) count++;
                        s[a][b] = false; // set pixel to white
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Check if all 5 conditions are satisfied
        /// </summary>
        private static bool SuenThinningAlg(int x, int y, bool[][] s, bool even)
        {
            bool p2 = s[x][y - 1];
            bool p3 = s[x + 1][y - 1];
            bool p4 = s[x + 1][y];
            bool p5 = s[x + 1][y + 1];
            bool p6 = s[x][y + 1];
            bool p7 = s[x - 1][y + 1];
            bool p8 = s[x - 1][y];
            bool p9 = s[x - 1][y - 1];


            int bp1 = NumberOfNonZeroNeighbors(x, y, s);
            if (bp1 >= 2 && bp1 <= 6) //2nd condition
            {
                if (NumberOfZeroToOneTransitionFromP9(x, y, s) == 1)
                {
                    if (even)
                    {
                        if (!((p2 && p4) && p8))
                        {
                            if (!((p2 && p6) && p8))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!((p2 && p4) && p6))
                        {
                            if (!((p4 && p6) && p8))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get the amount of transitions from white to black 
        /// </summary>
        private static int NumberOfZeroToOneTransitionFromP9(int x, int y, bool[][] s)
        {
            bool p2 = s[x][y - 1];
            bool p3 = s[x + 1][y - 1];
            bool p4 = s[x + 1][y];
            bool p5 = s[x + 1][y + 1];
            bool p6 = s[x][y + 1];
            bool p7 = s[x - 1][y + 1];
            bool p8 = s[x - 1][y];
            bool p9 = s[x - 1][y - 1];

            int A = Convert.ToInt32((!p2 && p3)) + Convert.ToInt32((!p3 && p4)) +
                    Convert.ToInt32((!p4 && p5)) + Convert.ToInt32((!p5 && p6)) +
                    Convert.ToInt32((!p6 && p7)) + Convert.ToInt32((!p7 && p8)) +
                    Convert.ToInt32((!p8 && p9)) + Convert.ToInt32((!p9 && p2));
            return A;
        }

        /// <summary>
        /// Get the amount of black pixel neighbours
        /// </summary>
        private static int NumberOfNonZeroNeighbors(int x, int y, bool[][] s)
        {
            int count = 0;
            if (s[x - 1][y]) count++;
            if (s[x - 1][y + 1]) count++;
            if (s[x - 1][y - 1]) count++;
            if (s[x][y + 1]) count++;
            if (s[x][y - 1]) count++;
            if (s[x + 1][y]) count++;
            if (s[x + 1][y + 1]) count++;
            if (s[x + 1][y - 1]) count++;
            return count;
        }

        /// <summary>
        /// Deep copy of the array
        /// </summary>
        private static T[][] ArrayClone<T>(T[][] A)
        {
            return A.Select(a => a.ToArray()).ToArray();
        }
    }
}
