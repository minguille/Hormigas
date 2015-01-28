using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FlickrNet;
using System.IO;

namespace Manina.Windows.Forms
{
    public partial class BackgroundWorkerForm : Form
    {
        private List<ImageListViewItem> m_list;
        private bool m_upload = false;
        private BackgroundWorker m_bw;
        private HormigasForm m_hf;
        private string m_folders_name = "";
        private int m_files_ok = 0;
        public BackgroundWorkerForm(HormigasForm hf, List<ImageListViewItem> list, bool upload)
        {
            m_hf = hf;
            m_list = list;
            m_upload = upload;
            InitializeComponent();
            SetUIChanges();

            m_bw = new BackgroundWorker();
            m_bw.WorkerSupportsCancellation = true;
            m_bw.WorkerReportsProgress = true;
            m_bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bw_DoWork);
            m_bw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bw_RunWorkerCompleted);
            m_bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
        }

        private void SetUIChanges()
        {   
            this.Text = HormigasForm.RM.GetString(m_upload ? "UPLOADING_FILES" : "COPYING_FILES");
            string msg = String.Format(HormigasForm.RM.GetString(m_upload ? "UPLOAD_QUESTION" : "COPY_QUESTION"), m_list.Count);
            labelBW.Text = msg;
            startBW.Text = HormigasForm.RM.GetString("START");
            cancelBW.Text = HormigasForm.RM.GetString("CANCEL");
        }

        private void startBW_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = m_list.Count;
            progressBar1.Value = 0;
            startBW.Enabled = false;
            m_bw.RunWorkerAsync(m_list);
        }

        private void cancelBW_Click(object sender, EventArgs e)
        {
            m_bw.CancelAsync();
            m_bw.Dispose();
            //this.Dispose();
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Set progress bar to 100% in case it's not already there.
            progressBar1.Value = progressBar1.Maximum;
            //progressBar1.Visible = false;
            List<ImageListViewItem> list_selectedItems = (List<ImageListViewItem>)e.Result;

            if (e.Error == null)
            {
                Console.WriteLine(m_upload ? "Upload Complete" : "Copy Complete");
                string msg = String.Format(HormigasForm.RM.GetString(m_upload ? "UPLOAD_OK":"COPY_OK"), m_files_ok, m_folders_name);
                labelBW.Text = msg;
                //MessageBox.Show(msg, HormigasForm.RM.GetString("INFORMATION"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Console.WriteLine(m_upload ? "Upload failed":"Copy failed");
                string msg = String.Format(HormigasForm.RM.GetString(m_upload ?"UPLOAD_ERROR":"COPY_ERROR"), e.Error.Message);
                labelBW.Text = msg;
                //MessageBox.Show(msg, HormigasForm.RM.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            list_selectedItems.Clear();
            GC.Collect();
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            BackgroundWorker worker = sender as BackgroundWorker;
            List<ImageListViewItem> list_selectedItems = (List<ImageListViewItem>)e.Argument;
            try
            {
                int progress = 0;
                foreach (ImageListViewItem item in list_selectedItems)
                {
                    if (worker.CancellationPending)
                    {
                        e.Result = list_selectedItems;
                        return;
                    }
                    if (m_upload)
                    {
                        string album = item.GetSubItemText(4);
                        string album_id = m_hf.findPhotoSetId(HormigasForm.FLICKR.PhotosetsGetList(), album);
                        PhotoSearchOptions options = new PhotoSearchOptions();
                        options.Text = m_hf.composeNewFileName(item);
                        options.Tags = item.GetSubItemText(1);
                        PhotoCollection photosFound = HormigasForm.FLICKR.PhotosSearch(options);
                        if (photosFound.Count > 0)
                            continue;
                        string newName = m_hf.composeNewFileName(item);
                        string photoId = HormigasForm.FLICKR.UploadPicture(item.FileName, newName, "", item.GetSubItemText(1), false, false, false);
                        if (album != "" && album_id == "")
                        {
                            Photoset set = HormigasForm.FLICKR.PhotosetsCreate(album, album, photoId);
                            album_id = set.PhotosetId;
                            if (m_folders_name.Length > 0)
                                m_folders_name += ",";
                            m_folders_name += album;
                        }
                        else if (album_id != "")
                            HormigasForm.FLICKR.PhotosetsAddPhoto(album_id, photoId);
                        m_files_ok++;
                        worker.ReportProgress(++progress, newName);
                    }
                    else
                    {
                        if (m_hf.getExcludeCheckedFiles() && item.Checked)
                            continue;
                        string path = m_hf.composeNewFilePathName(item);
                        string dir = Path.GetDirectoryName(path);
                        Directory.CreateDirectory(dir);
                        path = m_hf.getDuplicatedName(path);
                        //path = Path.Combine(path, newFileName);
                        File.Copy(item.FileName, path, true); // overwrite
                        worker.ReportProgress(++progress, path);
                        m_files_ok++;
                    }
                    
                }
                e.Result = list_selectedItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            this.Cursor = cursor;
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            labelBW.Text = "[" + progressBar1.Value.ToString() + " / " + progressBar1.Maximum.ToString() + "] " + (string)e.UserState;
        }
    }
}
