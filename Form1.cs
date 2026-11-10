using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jigsaw
{
    public partial class Form1 : Form
    {
        private List<Bitmap> puzzlePieces;
        private PictureBox dragPiece = null;
        private Point dragOffset = Point.Empty;

        private int rows = 3;
        private int cols = 3;
        private int pieceSize = 120;
        private int boardX = 80;
        private int boardY = 150;
        private int snapRange = 30;

        private bool[,] cellFilled = new bool[3, 3];
        private Random random = new Random();

        private int elapsedSeconds = 0;

        private Dictionary<PictureBox, int> pieceRotation = new Dictionary<PictureBox, int>();

        public Form1()
        {
            InitializeComponent();
            this.Paint += Form1_Paint;

            gameTimer.Interval = 1000;
            gameTimer.Enabled = false;

            lblTimer.Font = new Font("Arial", 10, FontStyle.Bold);
            lblPicture.Text = "Preview:";
            lblPicture.Font = new Font("Arial", 10, FontStyle.Bold);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawString("Puzzle Board",
                new Font("Arial", 10, FontStyle.Bold),
                Brushes.Black, boardX, boardY - 25);

            g.DrawString("Pieces Tray",
                new Font("Arial", 10, FontStyle.Bold),
                Brushes.Black, 500, boardY - 25);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Rectangle cell = new Rectangle(
                        boardX + col * pieceSize,
                        boardY + row * pieceSize,
                        pieceSize,
                        pieceSize);

                    if (cellFilled[row, col])
                        g.FillRectangle(Brushes.LightGreen, cell);
                    else
                        g.FillRectangle(Brushes.LightGray, cell);

                    if (!cellFilled[row, col])
                        g.DrawString(
                            (row * cols + col + 1).ToString(),
                            new Font("Arial", 18, FontStyle.Bold),
                            Brushes.DarkGray,
                            boardX + col * pieceSize + pieceSize / 2 - 10,
                            boardY + row * pieceSize + pieceSize / 2 - 15);

                    g.DrawRectangle(Pens.Black, cell);
                }
            }
        }

      
        private void btnload_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap original = new Bitmap(dialog.FileName);
                pictureBox1.Image = original;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                cellFilled = new bool[rows, cols];
                this.Invalidate();

                puzzlePieces = ImageSlicer.SliceImage(original, rows, cols);

                elapsedSeconds = 0;
                lblTimer.Text = "Time: 0s";
                gameTimer.Enabled = true;

                ShowPiecesOnForm();
                btnRestart.Enabled = true;
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            cellFilled = new bool[rows, cols];
            elapsedSeconds = 0;
            lblTimer.Text = "Time: 0s";
            gameTimer.Enabled = true;
            this.Invalidate();
            ShowPiecesOnForm();
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            lblTimer.Text = "Time: " + elapsedSeconds + "s";
        }

        private void ShowPiecesOnForm()
        {
            if (puzzlePieces == null) return;

            pieceRotation.Clear();

         
            List<Control> old = new List<Control>();
            foreach (Control c in this.Controls)
                if (c is PictureBox pb && pb.Name == "puzzlepiece")
                    old.Add(c);
            foreach (Control c in old)
                this.Controls.Remove(c);

            
            List<int> indices = new List<int>();
            for (int i = 0; i < puzzlePieces.Count; i++)
                indices.Add(i);
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

         
            int count = 0;
            foreach (int index in indices)
            {
                int x = 500 + (count % cols) * (pieceSize + 10) + random.Next(0, 10);
                int y = boardY + (count / cols) * (pieceSize + 10) + random.Next(0, 10);

                PictureBox pb = new PictureBox();
                pb.Name = "puzzlepiece";
                pb.Tag = index;
                pb.Image = puzzlePieces[index];
                pb.Size = new Size(pieceSize, pieceSize);
                pb.Location = new Point(x, y);
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.MouseDown += Piece_MouseDown;
                pb.MouseMove += Piece_MouseMove;
                pb.MouseUp += Piece_MouseUp;

                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem rotate = new ToolStripMenuItem("Rotate 90°");
                rotate.Click += (s, ev) => RotatePiece(pb);
                menu.Items.Add(rotate);
                pb.ContextMenuStrip = menu;

                this.Controls.Add(pb);
                pb.BringToFront();

                pieceRotation[pb] = 0;
                count++;
            }
        }

        private void RotatePiece(PictureBox pb)
        {
            if (pb.Image == null) return;

            Bitmap bmp = new Bitmap(pb.Image);
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pb.Image = bmp;

            pieceRotation[pb] = (pieceRotation[pb] + 90) % 360;
        }

        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            dragPiece = (PictureBox)sender;
            dragOffset = new Point(e.X, e.Y);
            dragPiece.BringToFront();
        }

        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragPiece == null) return;

            Point screen = dragPiece.PointToScreen(new Point(e.X, e.Y));
            Point client = this.PointToClient(screen);

            int newX = Math.Max(0, Math.Min(client.X - dragOffset.X,
                                this.ClientSize.Width - pieceSize));
            int newY = Math.Max(0, Math.Min(client.Y - dragOffset.Y,
                                this.ClientSize.Height - pieceSize));

            dragPiece.Location = new Point(newX, newY);
        }

        private void Piece_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragPiece == null) return;

            int index = int.Parse(dragPiece.Tag.ToString());
            int correctRow = index / cols;
            int correctCol = index % cols;
            int correctX = boardX + correctCol * pieceSize;
            int correctY = boardY + correctRow * pieceSize;

            int distX = Math.Abs(dragPiece.Left - correctX);
            int distY = Math.Abs(dragPiece.Top - correctY);

            bool rotationCorrect = pieceRotation.ContainsKey(dragPiece)
                                   && pieceRotation[dragPiece] == 0;

            if (distX < snapRange && distY < snapRange
                && !cellFilled[correctRow, correctCol]
                && rotationCorrect)
            {
                dragPiece.Location = new Point(correctX, correctY);
                dragPiece.MouseDown -= Piece_MouseDown;
                dragPiece.MouseMove -= Piece_MouseMove;
                dragPiece.MouseUp -= Piece_MouseUp;
                dragPiece.BorderStyle = BorderStyle.Fixed3D;
                dragPiece.ContextMenuStrip = null;

                cellFilled[correctRow, correctCol] = true;
                this.Invalidate();
                CheckWin();
            }

            dragPiece = null;
        }

        private void CheckWin()
        {
            for (int row = 0; row < rows; row++)
                for (int col = 0; col < cols; col++)
                    if (!cellFilled[row, col]) return;

            gameTimer.Enabled = false;

            MessageBox.Show(
                "Congratulations! Ilyas You solved the puzzle in " + elapsedSeconds + " seconds!",
                "You Win!", MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            btnRestart.Enabled = true;
        }
    }
}