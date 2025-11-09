// Archivo: OchoPuzzle/Form1.Designer.cs
namespace OchoPuzzle
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Variables para los controles
        private Button btnSolve;
        private Button btnShuffle;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // NOTA: El Form1_Load() ahora crea los botones dinámicamente.
        // Solo necesitamos declarar las variables aquí.
        private void InitializeComponent()
        {
            btnSolve = new Button();
            btnShuffle = new Button();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // btnSolve
            // 
            btnSolve.Location = new Point(0, 0);
            btnSolve.Name = "btnSolve";
            btnSolve.Size = new Size(75, 23);
            btnSolve.TabIndex = 0;
            // 
            // btnShuffle
            // 
            btnShuffle.Location = new Point(0, 0);
            btnShuffle.Name = "btnShuffle";
            btnShuffle.Size = new Size(75, 23);
            btnShuffle.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(100, 23);
            lblStatus.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(462, 337);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }
    }
}