﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MornaMapEditor
{
    public partial class FormTile : Form
    {

        private static readonly int pixelBuffer = 10;
        private Image fullTileRendering;
        private static readonly FormTile FormInstance = new FormTile();

        private readonly List<Point> selectedTiles = new List<Point>();
        private Point focusedTile = new Point(-1,-1);
        private int sizeModifier;
        private bool showGrid;
        
        public bool ShowGrid
        {
            get { return showGrid; }
            set { showGrid = value; Invalidate(); }
        }

        public static FormTile GetFormInstance()
        {
            return FormInstance;
        }

        private FormTile()
        {
            InitializeComponent();
            sizeModifier = ImageRenderer.Singleton.sizeModifier;
            this.MouseWheel += new MouseEventHandler(frmTile_MouseWheel);
            //this.Paint += frmTile_Paint;
        }

        private void frmTile_Load(object sender, EventArgs e)
        {
            menuStrip.Visible = false;
            MinimumSize = new Size(sizeModifier + pixelBuffer, sizeModifier + sb1.Height + menuStrip.Height + statusStrip.Height + pixelBuffer);
            RenderTileset();
        }

        private void RenderTileset()
        {
            if(fullTileRendering != null)
                fullTileRendering.Dispose();
            if (WindowState == FormWindowState.Minimized)
                return;

            //Bitmap tSet = new Bitmap(360, 360);
            int usableHeight = Height - sb1.Height - menuStrip.Height - statusStrip.Height - pixelBuffer;
            int tileRows = usableHeight / sizeModifier;
            int tilesPerRow = TileManager.Epf[0].max / tileRows;
            int currentRow = 0;
            int currentColumn = 0;

            sb1.Maximum = tilesPerRow;
            sb1.LargeChange = (Width / sizeModifier);
            Bitmap tmpBitmap = new Bitmap(tilesPerRow * sizeModifier, usableHeight);
            Graphics g = Graphics.FromImage(tmpBitmap);
            g.Clear(Color.DarkGreen);

            for (int tileNumber = 0; tileNumber < TileManager.Epf[0].max; tileNumber++)
            {
                int xPos =  currentColumn * sizeModifier;
                int yPos = currentRow * sizeModifier;
                g.DrawImage(ImageRenderer.Singleton.GetTileBitmap(tileNumber), xPos, yPos);

                currentColumn++;
                if (currentColumn > tilesPerRow)
                {
                    currentRow++;
                    currentColumn = 0;
                }
            }

            g.Dispose();
            fullTileRendering = tmpBitmap;
            Invalidate();
            //this.BackgroundImage = tSet;
            //picTileset.Image = tSet;
            //Application.DoEvents();
            //tSet = null;
        }

        private void sb1_Scroll(object sender, ScrollEventArgs e)
        {
            selectedTiles.Clear();
            Invalidate();
        }

        void formTile_Paint(object sender, PaintEventArgs e)
        {
            if (BackgroundImage != null)
                BackgroundImage.Dispose();
            int usableHeight = Height - sb1.Height - menuStrip.Height - statusStrip.Height + pixelBuffer;
            Bitmap tmpBitmap = new Bitmap(Width, usableHeight);
            Graphics graphics = Graphics.FromImage(tmpBitmap);

            Rectangle sourceRectangle = new Rectangle(sb1.Value * sizeModifier,0,Width,usableHeight);
            graphics.DrawImage(fullTileRendering, 0,0,sourceRectangle, GraphicsUnit.Pixel);
            BackgroundImage = tmpBitmap;
        }

        private void frmTile_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (sb1.Value - 1 >= sb1.Minimum) sb1.Value--;
            }
            else if (e.Delta < 0)
            {
                if (sb1.Value + 1 <= (sb1.Maximum - (Width / sizeModifier))) sb1.Value++;
            }

            sb1_Scroll(null, null); 
        }

        private void frmTile_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            int newSelectedTileX = e.X / sizeModifier;
            int newSelectedTileY = e.Y / sizeModifier;

            Point selectedTile = new Point(newSelectedTileX, newSelectedTileY);
            if (ModifierKeys == Keys.Control)
            {
                if (!selectedTiles.Contains(selectedTile))
                    selectedTiles.Add(selectedTile);
            }
            else
            {
                selectedTiles.Clear();
                selectedTiles.Add(selectedTile);
            }
            TileManager.TileSelection = GetSelection();
            TileManager.LastSelection = TileManager.SelectionType.Tile;
            //RenderTileset();
        }

        public void AdjustSizeModifier(int newModifier)
        {
            sizeModifier = newModifier;
            RenderTileset();
            Invalidate();
        }
        
        public Dictionary<Point, int> GetSelection()
        {
            Dictionary<Point, int> dictionary = new Dictionary<Point, int>();
            if (selectedTiles.Count == 0) return dictionary;

            int xMin = selectedTiles[0].X, yMin = selectedTiles[0].Y;

            foreach (Point selectedTile in selectedTiles)
            {
                if (xMin > selectedTile.X) xMin = selectedTile.X;
                if (yMin > selectedTile.Y) yMin = selectedTile.Y;
            }

            foreach (Point selectedTile in selectedTiles)
            {
                //dictionary.Add(new Point(selectedTile.X - xMin, selectedTile.Y - yMin), 
                //    GetTileNumber(selectedTile.X, selectedTile.Y));
            }

            return dictionary;
        }

        /*public void ClearSelection()
        {
            selectedTiles.Clear();
            this.Invalidate();
        }*/

        private void findTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumberInputForm numberInputForm = new NumberInputForm(@"Enter object number");
            if (numberInputForm.ShowDialog(this) == DialogResult.OK)
            {
               // NavigateToTile(numberInputForm.Number);
            }
        }

        private void showGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowGrid = showGridToolStripMenuItem.Checked;
        }

        private void FormTile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }

        }

        private void FormTile_ResizeEnd(object sender, EventArgs e)
        {
            //MessageBox.Show("Got a ResizeEnd");
            RenderTileset();
        }

        private void FormTile_Move(object sender, EventArgs e)
        {
         //   pausePainting = true;
        }
    }
}
