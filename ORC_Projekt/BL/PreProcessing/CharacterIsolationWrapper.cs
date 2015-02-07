using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL.PreProcessing
{
    class CharacterIsolationWrapper
    {
        /// <summary>
        /// indivdual images are stored in the harddrive in debug mode
        /// </summary>
        private const bool DEBUGMODE = true;

        /// <summary>
        /// nested class to store the dimentions of the boxes used to identify letters
        /// </summary>
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

        /// <summary>
        /// return isolated characters in a list of bitmaps
        /// </summary>
        public static List<Bitmap> IsolateCharacters(Bitmap input)
        {
            List<LetterBox> boxes = AnalyzeImage(input);
            return getIndividualCharactersFromBitmap(boxes, input);
        }

        /// <summary>
        /// crops the individual characters, defined by the boxes out of the input image
        /// </summary>
        private static List<Bitmap> getIndividualCharactersFromBitmap(List<LetterBox> boxes, Bitmap input)
        {
            List<Bitmap> chars = new List<Bitmap>();
            foreach (LetterBox box in boxes)
            {
                Bitmap croppedImage = input.Clone(new System.Drawing.Rectangle(box.x, box.y, box.width, box.height), input.PixelFormat);
                chars.Add(croppedImage);
                //if (DEBUGMODE)
                //{
                //    croppedImage.Save("character at position x" + box.x.ToString() + "y" + box.y.ToString() + ".png");
                //}
            }
            return chars;
        }

        /// <summary>
        /// Visualize the boxing process by drawing red squares around individual chars
        /// </summary>
        public static Bitmap VisualizeBoxing(Bitmap input)
        {
            List<LetterBox> boxes = AnalyzeImage(input);
            return drawBoxesIntoImage(boxes, input);
        }

        /// <summary>
        /// draw red squares with the dimentions of the boxes into the input bitmap
        /// </summary>
        private static Bitmap drawBoxesIntoImage(List<LetterBox> boxes, Bitmap input)
        {
            input = (Bitmap)input.Clone();
            foreach (LetterBox box in boxes)
            {
                for (int x = box.x; x <= box.x + box.width; x++)
                {
                    input.SetPixel(x, box.y, Color.Red);
                    input.SetPixel(x, box.y + box.height, Color.Red);
                }
                for (int y = box.y; y <= box.y + box.height; y++)
                {
                    input.SetPixel(box.x, y, Color.Red);
                    input.SetPixel(box.x + box.width, y, Color.Red);
                }
            }
            return input;
        }

        /// <summary>
        /// Analyzes the image and returns a list of boxes marking the dimentions of each character
        /// The condition for the input image for this to work is that there is only a single line of characters and each character is separated by enough white pixels
        /// </summary>
        private static List<LetterBox> AnalyzeImage(Bitmap input)
        {
            List<int> xCoordinatesOfWhiteColumns = GetPositionOfCompletelyWhiteColumns(input);
            List<LetterBox> letterBoxes = CalcLetterBoxes(input, xCoordinatesOfWhiteColumns);
            return letterBoxes;
        }

        /// <summary>
        /// Calculates the letterboxes out of the input image and the position of white columns that seperate each character
        /// </summary>
        private static List<LetterBox> CalcLetterBoxes(Bitmap input, List<int> xCoordinatesOfWhiteColumns)
        {
            int minLetterWith = 2;
            int minLetterHeight = 30;
            int boxOffset = 5;
            List<LetterBox> boxes = new List<LetterBox>();

            if (!xCoordinatesOfWhiteColumns.Any())
            {
                Console.WriteLine("input image does not fit the criteria. No white lines found");
                return boxes;
            }

            //calc x postion and width of the box
            int lastWhiteColumn = xCoordinatesOfWhiteColumns[0];
            int x = lastWhiteColumn;
            while ( x < input.Width)
            {
                if (!xCoordinatesOfWhiteColumns.Contains(x))
                {
                    bool foundLetter = true;
                    for (int i = x; i <= x + minLetterWith; i++)
                    {
                        if (xCoordinatesOfWhiteColumns.Contains(i))
                            foundLetter = false;
                    }
                    int indexOflastWhiteColumn = xCoordinatesOfWhiteColumns.IndexOf(lastWhiteColumn);
                    int nextWhiteColumn = xCoordinatesOfWhiteColumns[indexOflastWhiteColumn + 1];

                    int width = (nextWhiteColumn - x);
                    int boxX = x;
                    if ((x - boxOffset > 0) && ((nextWhiteColumn + boxOffset) <= input.Width))
                    {
                        boxX = x - boxOffset;
                        width += 2 * boxOffset;
                    }

                    if (foundLetter)
                    {
                        LetterBox box = new LetterBox(boxX, 0, width, 0);
                        boxes.Add(box);
                    }
                    x = nextWhiteColumn;
                    continue;
                }
                lastWhiteColumn = x;
                x++;
            }

            List<LetterBox> boxesToRemoveBecauseHeightIsToSmall = new List<LetterBox>();
            // calcYAndHeight
            foreach (LetterBox box in boxes)
            {
                int x1 = box.x;
                int x2 = box.x + box.width;
                int ymax = 0, ymin = input.Height;

                for (int currX = x1; currX < x2; currX++)
                {
                    for (int y = 0; y < input.Height; y++)
                    {
                        if (input.GetPixel(currX, y).R == 0)
                        {
                            if (y > ymax)
                                ymax = y;
                            if (y < ymin)
                                ymin = y;
                        }
                    }
                }

                int height = (ymax - ymin);
                if ((ymin - boxOffset > 0) && ((ymax + boxOffset) <= input.Height))
                {
                    ymin = ymin - boxOffset;
                    height += 2 * boxOffset;
                }
                box.y = ymin;
                box.height = height;
                if(height < minLetterHeight)
                {
                    boxesToRemoveBecauseHeightIsToSmall.Add(box);
                }
            }
            foreach (LetterBox box in boxesToRemoveBecauseHeightIsToSmall)
            {
                boxes.Remove(box);
            }
            return boxes;
        }

        /// <summary>
        /// return the x coordinates of columns that contain no black pixels
        /// </summary>
        private static List<int> GetPositionOfCompletelyWhiteColumns(Bitmap input)
        {
            List<int> xCoordinatesOfWhiteColumns = new List<int>();
            for (int x = 0; x < input.Width; x++)
            {
                bool isWhiteColumn = true;
                for (int y = 0; y < input.Height; y++)
                {
                    if (input.GetPixel(x, y).R != 255)
                    {
                        isWhiteColumn = false;
                        break;
                    }
                }
                if (isWhiteColumn)
                    xCoordinatesOfWhiteColumns.Add(x);
            }
            return xCoordinatesOfWhiteColumns;
        }
    }
}
