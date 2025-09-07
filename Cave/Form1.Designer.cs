namespace Cave
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pictureBox1 = new PictureBox();
            numSeed = new NumericUpDown();
            numOffs = new NumericUpDown();
            numIter = new NumericUpDown();
            numPerc = new NumericUpDown();
            lblSeed = new Label();
            lblOffset = new Label();
            lblIter = new Label();
            lblPerc = new Label();
            cbxSquareHex = new CheckBox();
            btnSnap = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIter).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOffs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPerc).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.DarkGreen;
            pictureBox1.Location = new Point(0, 60);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(657, 501);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            // 
            // lblSeed
            // 
            lblSeed.AutoSize = true;
            lblSeed.Location = new Point(0, 12);
            lblSeed.Name = "lblSeed";
            lblSeed.Size = new Size(42, 20);
            lblSeed.TabIndex = 1;
            lblSeed.Text = "Seed";
            // 
            // numSeed
            // 
            numSeed.Location = new Point(48, 12);
            numSeed.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numSeed.Name = "numSeed";
            numSeed.Size = new Size(81, 27);
            numSeed.TabIndex = 2;
            // 
            // lblOffset
            // 
            lblOffset.AutoSize = true;
            lblOffset.Location = new Point(144, 12);
            lblOffset.Name = "lblOffset";
            lblOffset.Size = new Size(49, 20);
            lblOffset.TabIndex = 3;
            lblOffset.Text = "Offset";
            // 
            // numOffs
            // 
            numOffs.Location = new Point(199, 12);
            numOffs.Name = "numOffs";
            numOffs.Size = new Size(50, 27);
            numOffs.TabIndex = 4;
            // 
            // lblIter
            // 
            lblIter.AutoSize = true;
            lblIter.Location = new Point(254, 12);
            lblIter.Name = "lblIter";
            lblIter.Size = new Size(71, 20);
            lblIter.TabIndex = 5;
            lblIter.Text = "Iterations";
            // 
            // numIter
            // 
            numIter.Location = new Point(331, 12);
            numIter.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
            numIter.Name = "numIter";
            numIter.Size = new Size(50, 27);
            numIter.TabIndex = 6;
            numIter.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblPerc
            // 
            lblPerc.AutoSize = true;
            lblPerc.Location = new Point(389, 12);
            lblPerc.Name = "lblPerc";
            lblPerc.Size = new Size(38, 20);
            lblPerc.TabIndex = 7;
            lblPerc.Text = "Wall";
            // 
            // numPerc
            // 
            numPerc.Location = new Point(433, 12);
            numPerc.Name = "numPerc";
            numPerc.Size = new Size(50, 27);
            numPerc.TabIndex = 8;
            // 
            // cbxSquareHex
            // 
            cbxSquareHex.Appearance = Appearance.Button;
            cbxSquareHex.AutoSize = true;
            cbxSquareHex.Location = new Point(489, 19);
            cbxSquareHex.Margin = new Padding(3, 4, 3, 4);
            cbxSquareHex.Name = "cbxSquareHex";
            cbxSquareHex.Size = new Size(65, 30);
            cbxSquareHex.TabIndex = 9;
            cbxSquareHex.Text = "Square";
            cbxSquareHex.UseVisualStyleBackColor = true;
            cbxSquareHex.CheckedChanged += cbxSquareHex_CheckedChanged;
            // 
            // btnSnap
            // 
            btnSnap.Location = new Point(556, 19);
            btnSnap.Name = "btnSnap";
            btnSnap.Size = new Size(85, 30);
            btnSnap.TabIndex = 10;
            btnSnap.Text = "Snap";
            btnSnap.UseVisualStyleBackColor = true;
            btnSnap.Click += btnSnap_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(814, 793);
            Controls.Add(btnSnap);
            Controls.Add(cbxSquareHex);
            Controls.Add(lblSeed);
            Controls.Add(lblOffset);
            Controls.Add(lblIter);
            Controls.Add(lblPerc);
            Controls.Add(numSeed);
            Controls.Add(numOffs);
            Controls.Add(numIter);
            Controls.Add(numPerc);
            Controls.Add(pictureBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Cellular Automata";
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIter).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOffs).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPerc).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private NumericUpDown numSeed;
        private NumericUpDown numOffs;
        private NumericUpDown numIter;
        private NumericUpDown numPerc;
        private Label lblSeed;
        private Label lblOffset;
        private Label lblIter;
        private Label lblPerc;
        private CheckBox cbxSquareHex;
        private Button btnSnap;
    }
}