﻿namespace NSMBe4 {
    partial class PalettePicker {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // PalettePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PalettePicker";
            this.Size = new System.Drawing.Size(192, 192);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PalettePicker_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PalettePicker_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PalettePicker_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PalettePicker_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}