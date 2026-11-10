using System.Collections.Generic;
using System.Drawing;

namespace Jigsaw
{
    public class ImageSlicer
    {
        public static List<Bitmap> SliceImage(Bitmap source, int rows, int cols)
        {
            List<Bitmap> pieces = new List<Bitmap>();

            int pieceWidth = source.Width / cols;
            int pieceHeight = source.Height / rows;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Bitmap piece = new Bitmap(pieceWidth, pieceHeight);

                    using (Graphics g = Graphics.FromImage(piece))
                    {
                        Rectangle srcRect = new Rectangle(
                            col * pieceWidth,
                            row * pieceHeight,
                            pieceWidth,
                            pieceHeight);

                        Rectangle destRect = new Rectangle(
                            0, 0, pieceWidth, pieceHeight);

                        g.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
                    }

                    pieces.Add(piece);
                }
            }

            return pieces;
        }
    }
}