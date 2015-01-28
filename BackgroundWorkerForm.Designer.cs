namespace Manina.Windows.Forms
{
    partial class BackgroundWorkerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackgroundWorkerForm));
            this.startBW = new System.Windows.Forms.Button();
            this.cancelBW = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelBW = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startBW
            // 
            this.startBW.Location = new System.Drawing.Point(13, 85);
            this.startBW.Name = "startBW";
            this.startBW.Size = new System.Drawing.Size(75, 23);
            this.startBW.TabIndex = 0;
            this.startBW.Text = "START";
            this.startBW.UseVisualStyleBackColor = true;
            this.startBW.Click += new System.EventHandler(this.startBW_Click);
            // 
            // cancelBW
            // 
            this.cancelBW.Location = new System.Drawing.Point(309, 84);
            this.cancelBW.Name = "cancelBW";
            this.cancelBW.Size = new System.Drawing.Size(75, 23);
            this.cancelBW.TabIndex = 1;
            this.cancelBW.Text = "CANCEL";
            this.cancelBW.UseVisualStyleBackColor = true;
            this.cancelBW.Click += new System.EventHandler(this.cancelBW_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 56);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(371, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // labelBW
            // 
            this.labelBW.AutoSize = true;
            this.labelBW.Location = new System.Drawing.Point(13, 13);
            this.labelBW.Name = "labelBW";
            this.labelBW.Size = new System.Drawing.Size(35, 13);
            this.labelBW.TabIndex = 3;
            this.labelBW.Text = "label1";
            // 
            // BackgroundWorkerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 114);
            this.Controls.Add(this.labelBW);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cancelBW);
            this.Controls.Add(this.startBW);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BackgroundWorkerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BackgroundWorkerForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startBW;
        private System.Windows.Forms.Button cancelBW;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelBW;
    }
}