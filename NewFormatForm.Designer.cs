namespace Manina.Windows.Forms
{
    partial class NewFormatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFormatForm));
            this.labelFileName = new System.Windows.Forms.Label();
            this.labelFileNameItem = new System.Windows.Forms.Label();
            this.labelTag = new System.Windows.Forms.Label();
            this.labelTagItem = new System.Windows.Forms.Label();
            this.labelYearItem = new System.Windows.Forms.Label();
            this.labelYear = new System.Windows.Forms.Label();
            this.labelMonthItem = new System.Windows.Forms.Label();
            this.labelMonth = new System.Windows.Forms.Label();
            this.labelDayItem = new System.Windows.Forms.Label();
            this.labelDay = new System.Windows.Forms.Label();
            this.labelTimeItem = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.textBoxFormat = new System.Windows.Forms.TextBox();
            this.labelFormatted = new System.Windows.Forms.Label();
            this.checkBoxDefaultFormat = new System.Windows.Forms.CheckBox();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelFileName
            // 
            this.labelFileName.Location = new System.Drawing.Point(16, 13);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(118, 23);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "File Name:";
            this.labelFileName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelFileNameItem
            // 
            this.labelFileNameItem.AutoSize = true;
            this.labelFileNameItem.Location = new System.Drawing.Point(139, 13);
            this.labelFileNameItem.Name = "labelFileNameItem";
            this.labelFileNameItem.Size = new System.Drawing.Size(114, 13);
            this.labelFileNameItem.TabIndex = 1;
            this.labelFileNameItem.Text = "Example File Name.jpg";
            // 
            // labelTag
            // 
            this.labelTag.Location = new System.Drawing.Point(16, 36);
            this.labelTag.Name = "labelTag";
            this.labelTag.Size = new System.Drawing.Size(118, 13);
            this.labelTag.TabIndex = 2;
            this.labelTag.Text = "Tag:";
            this.labelTag.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelTagItem
            // 
            this.labelTagItem.AutoSize = true;
            this.labelTagItem.Location = new System.Drawing.Point(139, 36);
            this.labelTagItem.Name = "labelTagItem";
            this.labelTagItem.Size = new System.Drawing.Size(106, 13);
            this.labelTagItem.TabIndex = 3;
            this.labelTagItem.Text = "Example Tag1, Tag2";
            // 
            // labelYearItem
            // 
            this.labelYearItem.AutoSize = true;
            this.labelYearItem.Location = new System.Drawing.Point(139, 59);
            this.labelYearItem.Name = "labelYearItem";
            this.labelYearItem.Size = new System.Drawing.Size(31, 13);
            this.labelYearItem.TabIndex = 5;
            this.labelYearItem.Text = "2000";
            // 
            // labelYear
            // 
            this.labelYear.Location = new System.Drawing.Point(16, 59);
            this.labelYear.Name = "labelYear";
            this.labelYear.Size = new System.Drawing.Size(118, 13);
            this.labelYear.TabIndex = 4;
            this.labelYear.Text = "Year:";
            this.labelYear.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelMonthItem
            // 
            this.labelMonthItem.AutoSize = true;
            this.labelMonthItem.Location = new System.Drawing.Point(139, 82);
            this.labelMonthItem.Name = "labelMonthItem";
            this.labelMonthItem.Size = new System.Drawing.Size(19, 13);
            this.labelMonthItem.TabIndex = 7;
            this.labelMonthItem.Text = "10";
            // 
            // labelMonth
            // 
            this.labelMonth.Location = new System.Drawing.Point(16, 82);
            this.labelMonth.Name = "labelMonth";
            this.labelMonth.Size = new System.Drawing.Size(118, 13);
            this.labelMonth.TabIndex = 6;
            this.labelMonth.Text = "Month:";
            this.labelMonth.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelDayItem
            // 
            this.labelDayItem.AutoSize = true;
            this.labelDayItem.Location = new System.Drawing.Point(139, 105);
            this.labelDayItem.Name = "labelDayItem";
            this.labelDayItem.Size = new System.Drawing.Size(19, 13);
            this.labelDayItem.TabIndex = 9;
            this.labelDayItem.Text = "20";
            // 
            // labelDay
            // 
            this.labelDay.Location = new System.Drawing.Point(16, 105);
            this.labelDay.Name = "labelDay";
            this.labelDay.Size = new System.Drawing.Size(118, 13);
            this.labelDay.TabIndex = 8;
            this.labelDay.Text = "Day:";
            this.labelDay.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelTimeItem
            // 
            this.labelTimeItem.AutoSize = true;
            this.labelTimeItem.Location = new System.Drawing.Point(139, 128);
            this.labelTimeItem.Name = "labelTimeItem";
            this.labelTimeItem.Size = new System.Drawing.Size(61, 13);
            this.labelTimeItem.TabIndex = 11;
            this.labelTimeItem.Text = "192350123";
            // 
            // labelTime
            // 
            this.labelTime.Location = new System.Drawing.Point(16, 128);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(118, 13);
            this.labelTime.TabIndex = 10;
            this.labelTime.Text = "Time:";
            this.labelTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxFormat
            // 
            this.textBoxFormat.Location = new System.Drawing.Point(13, 155);
            this.textBoxFormat.Name = "textBoxFormat";
            this.textBoxFormat.Size = new System.Drawing.Size(521, 20);
            this.textBoxFormat.TabIndex = 12;
            this.textBoxFormat.TextChanged += new System.EventHandler(this.textBoxFormat_TextChanged);
            // 
            // labelFormatted
            // 
            this.labelFormatted.AutoSize = true;
            this.labelFormatted.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFormatted.Location = new System.Drawing.Point(13, 187);
            this.labelFormatted.Name = "labelFormatted";
            this.labelFormatted.Size = new System.Drawing.Size(0, 13);
            this.labelFormatted.TabIndex = 13;
            // 
            // checkBoxDefaultFormat
            // 
            this.checkBoxDefaultFormat.AutoSize = true;
            this.checkBoxDefaultFormat.Location = new System.Drawing.Point(95, 216);
            this.checkBoxDefaultFormat.Name = "checkBoxDefaultFormat";
            this.checkBoxDefaultFormat.Size = new System.Drawing.Size(90, 17);
            this.checkBoxDefaultFormat.TabIndex = 14;
            this.checkBoxDefaultFormat.Text = "Make Default";
            this.checkBoxDefaultFormat.UseVisualStyleBackColor = true;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Location = new System.Drawing.Point(13, 212);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 23);
            this.buttonAccept.TabIndex = 15;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(459, 212);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // NewFormatForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(546, 250);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.checkBoxDefaultFormat);
            this.Controls.Add(this.labelFormatted);
            this.Controls.Add(this.textBoxFormat);
            this.Controls.Add(this.labelTimeItem);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.labelDayItem);
            this.Controls.Add(this.labelDay);
            this.Controls.Add(this.labelMonthItem);
            this.Controls.Add(this.labelMonth);
            this.Controls.Add(this.labelYearItem);
            this.Controls.Add(this.labelYear);
            this.Controls.Add(this.labelTagItem);
            this.Controls.Add(this.labelTag);
            this.Controls.Add(this.labelFileNameItem);
            this.Controls.Add(this.labelFileName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "NewFormatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NewFormatForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.Label labelFileNameItem;
        private System.Windows.Forms.Label labelTag;
        private System.Windows.Forms.Label labelTagItem;
        private System.Windows.Forms.Label labelYearItem;
        private System.Windows.Forms.Label labelYear;
        private System.Windows.Forms.Label labelMonthItem;
        private System.Windows.Forms.Label labelMonth;
        private System.Windows.Forms.Label labelDayItem;
        private System.Windows.Forms.Label labelDay;
        private System.Windows.Forms.Label labelTimeItem;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.TextBox textBoxFormat;
        private System.Windows.Forms.Label labelFormatted;
        private System.Windows.Forms.CheckBox checkBoxDefaultFormat;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonCancel;
    }
}