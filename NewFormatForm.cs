using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Manina.Windows.Forms
{
    public partial class NewFormatForm : Form
    {
        private HormigasForm m_hf;
        private ImageListViewItem m_item = null;
        private string m_extension;
        private bool m_isAlbum;
        public string ReturnFormat {get;set;} 
        public bool ReturnDefault  {get;set;} 
        public NewFormatForm(HormigasForm hf, ImageListViewItem item, string oldFormat, bool isAlbum=false)
        {
            m_hf = hf;
            m_item = item;
            m_isAlbum = isAlbum;
            InitializeComponent();
            SetUIChanges();
            textBoxFormat.Text = oldFormat;
        }

        private void SetUIChanges()
        {
            this.Text = HormigasForm.RM.GetString("NEW_FORMAT");
            labelFileName.Text = HormigasForm.RM.GetString("FILE_NAME");
            labelTag.Text = HormigasForm.RM.GetString("FORMAT_TAG");
            labelYear.Text = HormigasForm.RM.GetString("YEAR");
            labelMonth.Text = HormigasForm.RM.GetString("MONTH");
            labelDay.Text = HormigasForm.RM.GetString("DAY");
            labelTime.Text = HormigasForm.RM.GetString("TIME");
            checkBoxDefaultFormat.Text = HormigasForm.RM.GetString("MAKE_DEFAULT");

            labelYearItem.Text = "";
            labelMonthItem.Text = "";
            labelDayItem.Text = "";
            labelTimeItem.Text = "";

            if (m_item == null)
            {
                labelFileNameItem.Text = HormigasForm.RM.GetString("EXAMPLE_FILENAME");
                m_extension = Path.GetExtension(labelFileNameItem.Text);
                if (m_extension.Length > 0)
                    labelFileNameItem.Text = labelFileNameItem.Text.Replace(m_extension, "");
                labelTagItem.Text = HormigasForm.RM.GetString("EXAMPLE_TAG");
                labelYearItem.Text = HormigasForm.RM.GetString("EXAMPLE_YEAR");
                labelMonthItem.Text = HormigasForm.RM.GetString("EXAMPLE_MONTH");
                labelDayItem.Text = HormigasForm.RM.GetString("EXAMPLE_DAY");
                labelTimeItem.Text = HormigasForm.RM.GetString("EXAMPLE_TIME");
            }
            else
            {
                labelFileNameItem.Text = m_item.Text;
                m_extension = Path.GetExtension(m_item.Text);
                if (m_extension.Length > 0)
                    labelFileNameItem.Text = labelFileNameItem.Text.Replace(m_extension, "");
                labelTagItem.Text = m_item.GetSubItemText(1);
                DateTime dt = DateTime.Now;
                if (m_hf.getDateToCompare(m_item, ref dt))
                {
                    labelYearItem.Text = dt.Year.ToString();
                    labelMonthItem.Text = dt.Month.ToString("D2");
                    labelDayItem.Text = dt.Day.ToString("D2");
                    labelTimeItem.Text = dt.Hour.ToString("D2") +
                                         dt.Minute.ToString("D2") +
                                         dt.Second.ToString("D2") +
                                         dt.Millisecond.ToString("D3");
                }
            }
        }

        private void textBoxFormat_TextChanged(object sender, EventArgs e)
        {
            string newName = m_hf.parseFormat(((TextBox)sender).Text,
                 labelFileNameItem.Text,
                 labelTagItem.Text,
                 labelYearItem.Text,
                 labelMonthItem.Text,
                 labelDayItem.Text,
                 labelTimeItem.Text,!m_isAlbum);
            labelFormatted.Text = newName;
            if (m_isAlbum)
                checkBoxDefaultFormat.Visible = textBoxFormat.Text != m_hf.getDefaultAlbum();
            else
                checkBoxDefaultFormat.Visible = textBoxFormat.Text != m_hf.getDefaultFormat();
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.ReturnDefault = checkBoxDefaultFormat.Checked;
            this.ReturnFormat = textBoxFormat.Text;
            this.Close();
        }
    }
}
