using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL
{
    class CharacterIsolation
    {
        private class LetterBox
        {
            public LetterBox(int _x, int _y, int _width, int _height)
            {
                x = _x;
                y = _y;
                width = _width;
                height = _height;
            }
            public int x;
            public int y;
            public int width;
            public int height;
        }
        public static Bitmap[] IsolateCharacters(Bitmap input)
        {
            AnalyzeImage(input);
            return null;

        }
        public static Bitmap VisualizeBoxing(Bitmap input)
        {
            List<LetterBox> boxes = AnalyzeImage(input);
            return drawBoxesIntoImage(boxes,input);
        }

        private static Bitmap drawBoxesIntoImage(List<LetterBox> boxes, Bitmap input)
        {
            
            foreach (LetterBox box in boxes)
            {
                Pen myPen = new Pen(System.Drawing.Color.Red, 5);
                Graphics g = Graphics.FromImage(input);
                g.DrawRectangle(myPen,box.x,box.y,box.width,box.height);
                g.Dispose();
            }
            return input;
        }

        private static List<LetterBox> AnalyzeImage(Bitmap input)
        {
            List<int> xCoordinatesOfWhiteColumns = GetPositionOfCompletelyWhiteColumns(input);
            List<LetterBox> letterBoxes = CalcLetterBoxes(input, xCoordinatesOfWhiteColumns);
            return letterBoxes;
        }

        private static List<LetterBox> CalcLetterBoxes(Bitmap input, List<int> xCoordinatesOfWhiteColumns)
        {
            int minLetterWith = 3;
            List<LetterBox> boxes = new List<LetterBox>();

            //calcXAndWidth
            int previousX = 0;
            for (int x = 0; x < input.Width; x++)
            {
                if (!xCoordinatesOfWhiteColumns.Contains(x))
                {
                    bool foundLetter = true;
                    for (int i = x; i <= x + minLetterWith; i++)
                    {
                        if (xCoordinatesOfWhiteColumns.Contains(i))
                            foundLetter = false;
                    }
                    int index = xCoordinatesOfWhiteColumns.IndexOf(previousX);
                    int nextX = xCoordinatesOfWhiteColumns[index + 1];
                    int width = nextX - x;

                    if(foundLetter)
                    {
                        LetterBox box = new LetterBox(x, 0, width, 0);
                        boxes.Add(box);
                    }
                    x = nextX;
                    continue;
                }
                previousX = x;
            }

            // calcYAndHeight
            foreach (LetterBox box in boxes)
            {
                int x1 = box.x;
                int x2 = box.x + box.width;
                int ymax = 0, ymin = input.Height;
                
                for(int x = x1; x < x2; x++)
                {
                    for(int y=0; y < input.Height ; y++)
                    {
                        if(input.GetPixel(x,y).R == 0)
                        {
                            if(y > ymax)
                                ymax = y;
                            if (y < ymin)
                                ymin = y;
                        }
                    }
                }
                box.y = ymin;
                box.height = ymax - ymin;
            }
            return boxes;
        }

        private static List<int> GetPositionOfCompletelyWhiteColumns(Bitmap input)
        {
            List<int> xCoordinatesOfWhiteColumns = new List<int>();
            for (int x = 0; x < input.Width; x++)
            {
                bool isWhiteColumn = true;
                for (int y = 0; y < input.Height; y++)
                {
                    if (input.GetPixel(x, y).R != 255)
                        isWhiteColumn = false;
                }
                if (isWhiteColumn)
                    xCoordinatesOfWhiteColumns.Add(x);
            }
            return xCoordinatesOfWhiteColumns;
        }
    }
}
