namespace GalletaAI
{
    // Archivo: Form1.Designer.cs
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnNewGame;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblStatus = new Label();
            btnNewGame = new Button();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStatus.Location = new Point(14, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(177, 28);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Humano: 0 | IA: 0";
            // 
            // btnNewGame
            // 
            btnNewGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewGame.Location = new Point(397, 12);
            btnNewGame.Margin = new Padding(3, 4, 3, 4);
            btnNewGame.Name = "btnNewGame";
            btnNewGame.Size = new Size(109, 37);
            btnNewGame.TabIndex = 1;
            btnNewGame.Text = "Juego Nuevo";
            btnNewGame.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(519, 601);
            Controls.Add(btnNewGame);
            Controls.Add(lblStatus);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Juego de la Galleta (Dots and Boxes)";
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}