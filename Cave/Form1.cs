namespace Cave
{
    /// <summary>
    /// UI to display the "CaveStream" RAM map based on input.
    /// </summary>
    /// <remarks>
    /// This UI by David R Ross 7/22/2025.
    /// </remarks>
    public partial class Form1 : Form
    {
        private const float HEX_DISTANCE_FACTOR = 1.15470054F; // hardcode of "2 / Math.Sqrt(3)" in floatingpoint
        private const int SQUARE_OFFSET = 40;
        private const int HEX_OFFSET = 30;

        private ulong seed, offs;
        private int iters, contentwidth, contentheight, offset;
        private byte wallperc;
        private CaveStream _caveStream;

        public Form1()
        {
            InitializeComponent();

            seed = 0;
            offs = 0;
            iters = 4;
            wallperc = 18;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numSeed.Value = seed;
            numIter.Value = iters;
            numOffs.Value = offs;
            numPerc.Value = wallperc;
            numSeed.ValueChanged += numSeed_ValueChanged;
            numPerc.ValueChanged += numPerc_ValueChanged;
            numOffs.ValueChanged += numOffs_ValueChanged;
            numIter.ValueChanged += numIter_ValueChanged;
            this.OnResize(EventArgs.Empty); //call Form1_Resize, as to fit picturebox to form
            Snap();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Width = Width;
            pictureBox1.Height = Height - pictureBox1.Top;
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            bool isHex = cbxSquareHex.Checked;
            var walls = _caveStream.GetMap();
            var g = e.Graphics;
            //var pen = new Pen(Color.LightGreen, 1F) for DrawRectangle
            SolidBrush brushWall = new SolidBrush(Color.LightGreen),
                brushBubble = new SolidBrush(Color.DarkGreen);

            //top to bottom
            for (int y = 0; y < contentheight; y++)
            {
                //left to right
                for (int x = 0; x < contentwidth; x++)
                {
                    var brush = !walls[x, y] ? brushWall : brushBubble; //false is wall; true is hole in wall
                    if (isHex)
                        g.FillPolygon(brush, HexToPoints(offset, x, y));
                    else g.FillRectangle(brush, x * offset, y * offset, offset, offset);
                }
            }
        }

        private void numSeed_ValueChanged(object sender, EventArgs e)
        {
            var newseed = (ulong)numSeed.Value;
            short forwardSeed = (short)(newseed - seed);
            seed = newseed;
            if (forwardSeed == 1)//> 0 && forwardSeed < contentheight - 2)
            {
                //iters = 0; //TROUBLESHOOT the rnd. of course snap is broken until we do this. right now changing the wall%s will reset
                _caveStream.BumpSeed(forwardSeed);
            }
            else
            {
                _caveStream.RedoRandom(newseed, offs);
                _caveStream.SetFirstMap(wallperc);
                _caveStream.Iterate(iters);
            }
            pictureBox1.Refresh();
        }
        private void numOffs_ValueChanged(object sender, EventArgs e)
        {
            offs = (ulong)numOffs.Value;
            _caveStream.RedoRandom(seed, offs);
            _caveStream.SetFirstMap(wallperc);
            _caveStream.Iterate(iters);
            pictureBox1.Refresh();
        }

        private void numIter_ValueChanged(object sender, EventArgs e)
        {
            int newiters = (int)numIter.Value;
            int forwardIters = newiters - iters;
            iters = newiters;
            if (forwardIters > 0)
            {
                _caveStream.Iterate(forwardIters);
            }
            else
            {
                //decreasing is a reversal of entropy, so have to clear and restart like when Perc changes
                _caveStream.SetFirstMap(wallperc);
                _caveStream.Iterate(iters);
            }
            pictureBox1.Refresh();
        }

        private void numPerc_ValueChanged(object sender, EventArgs e)
        {
            wallperc = (byte)numPerc.Value;
            pictureBox1.BackColor = wallperc <= 50 ? Color.DarkGreen : Color.LightGreen;
            _caveStream.SetFirstMap(wallperc);
            _caveStream.Iterate(iters);
            pictureBox1.Refresh();
        }
        private void cbxSquareHex_CheckedChanged(object sender, EventArgs e)
        {
            cbxSquareHex.Text = cbxSquareHex.Checked ? "Hex" : "Square";
            ChangeCave();
            pictureBox1.Refresh();
        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            Snap();
            pictureBox1.Refresh();
        }

        private void ChangeCave()
        {
            _caveStream = new CaveStream(cbxSquareHex.Checked, (uint)contentwidth, (uint)contentheight, seed, offs, wallperc);
            _caveStream.Iterate(iters);
        }
        private void Snap()
        {
            contentwidth = pictureBox1.Width * iters / 35;
            contentwidth -= contentwidth % 8; // 64-bit PRNG
            contentwidth += 4;
            contentheight = pictureBox1.Height * iters / 35;
            bool isHex = cbxSquareHex.Checked;
            if (!isHex) offset = iters >= SQUARE_OFFSET ? 1 : SQUARE_OFFSET / (iters + 1); //so past OFFSET iters, pixel-to-value is 1. but my RAM bogs down ~25 anyway.
            else
                offset = iters >= HEX_OFFSET ? 1 : HEX_OFFSET / (iters + 1);
            ChangeCave();
        }

        /// <summary>
        /// Return point-array of a point-vertical hexagon.
        /// </summary>
        /// <param name="distance">Edge-to-edge as float.</param>
        /// <param name="col">Position-X [diagonal]</param>
        /// <param name="row">Position-Y</param>
        /// <returns>Array of the hexagon's six points.</returns>
        /// <remarks>
        /// Inspired by Rocky Mountain Computer Consulting,
        /// https://www.csharphelper.com/howtos/howto_hexagonal_grid.html (point-horiz).
        /// In D&D, travel is done from edge-to-edge.
        /// I call it "distance" rather than anything orientation-specific.
        /// </remarks>
        private PointF[] HexToPoints(float distance, float col, float row)
        {
            //RMCC started hexagons from upperleft so I'll do it too.
            //Start each point-vertical hex with upperleft corner (not top).

            //vertex-to-vertex width, based on edge-to-edge distance, as a floatingpoint, for PointF.
            float height = HEX_DISTANCE_FACTOR * distance;

            float
                x = col * distance
                    + (row % 2 == 1 ? distance / 2 : 0),// If the row (y) is odd, bump across half a hex more.
                y = row * height * 0.75f
                    + HEX_DISTANCE_FACTOR / 4f;

            //Gygaxify.
            x += row * height;
            x = 500 - x;

            return
                [
            new PointF(x, y),
            new PointF(x + distance * 0.5f, y - height * 0.25f),
            new PointF(x + distance, y),
            new PointF(x + distance, y + height * 0.5f),
            new PointF(x + distance * 0.5f, y + height * 0.75f),
            new PointF(x, y + height * 0.5f),
                ];
        }
    }
}
