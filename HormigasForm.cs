using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using FlickrNet;
using System.Net;
using System.Text;

namespace Manina.Windows.Forms
{
    public partial class HormigasForm : Form
    {
        #region Member variables
        private static bool m_populatingView = false;
        private string m_strResourcesPath = Application.StartupPath + Path.DirectorySeparatorChar + "Resources";
        private string m_default_format = "\\%a\\%a_%m\\%a_%m_%e_%d_%h";
        private string m_default_album = "%a_%m_%e";
        private string m_strCulture = "es-ES";
        private string m_objectsFile = "Hormigas.dat";
        private string m_defaultFormatKey = "HormigasDefaultFormatKey.dat";
        private string m_defaultAlbumKey = "HormigasDefaultAlbumKey.dat";
        private static List<string> m_lDirsToShow;
        private static Dictionary<string, HormigaObject> m_dObjects;
        private static ResourceManager m_rm;
        // flickr
        private static Flickr m_flickr = null;
        private static string m_ApiKey = "763bcf46a01a7bcb1cf7fda44d0282ac";
        private static string m_SharedSecret = "8009b7cda9d9919f";
        OAuthRequestToken m_requestToken = null;
        OAuthAccessToken m_accessToken = null;
        #endregion

        #region Renderer and color combobox items
        /// <summary>
        /// Represents an item in the renderer combobox.
        /// </summary>
        private struct RendererComboBoxItem
        {
            public string Name;
            public string FullName;

            public override string ToString()
            {
                return Name;
            }

            public RendererComboBoxItem(Type type)
            {
                Name = type.Name;
                FullName = type.FullName;
            }
        }

        /// <summary>
        /// Represents an item in the custom color combobox.
        /// </summary>
        private struct ColorComboBoxItem
        {
            public string Name;
            public PropertyInfo Field;

            public override string ToString()
            {
                return Name;
            }

            public ColorComboBoxItem(PropertyInfo field)
            {
                Name = field.Name;
                Field = field;
            }
        }
        #endregion

        #region Constructor
        public HormigasForm()
        {
            InitializeComponent();

            // Setup the background worker
            Application.Idle += new EventHandler(Application_Idle);

            // Find and add built-in renderers
            Assembly assembly = Assembly.GetAssembly(typeof(ImageListView));
            int i = 0;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType == typeof(ImageListView.ImageListViewRenderer))
                {
                    renderertoolStripComboBox.Items.Add(new RendererComboBoxItem(type));
                    if (type.Name == "DefaultRenderer")
                        renderertoolStripComboBox.SelectedIndex = i;
                    i++;
                }
            }
            // Find and add custom colors
            Type colorType = typeof(ImageListViewColor);
            i = 0;
            foreach (PropertyInfo field in colorType.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                colorToolStripComboBox.Items.Add(new ColorComboBoxItem(field));
                if (field.Name == "Default")
                    colorToolStripComboBox.SelectedIndex = i;
                i++;
            }
            // Dynamically add aligment values
            foreach (object o in Enum.GetValues(typeof(ContentAlignment)))
            {
                ToolStripMenuItem item1 = new ToolStripMenuItem(o.ToString());
                item1.Tag = o;
                item1.Click += new EventHandler(checkboxAlignmentToolStripButton_Click);
                checkboxAlignmentToolStripMenuItem.DropDownItems.Add(item1);
                ToolStripMenuItem item2 = new ToolStripMenuItem(o.ToString());
                item2.Tag = o;
                item2.Click += new EventHandler(iconAlignmentToolStripButton_Click);
                iconAlignmentToolStripMenuItem.DropDownItems.Add(item2);
            }

            m_dObjects = new Dictionary<string, HormigaObject>();
            m_lDirsToShow = new List<string>();

            imageListView1.AllowDuplicateFileNames = true;
            imageListView1.SetRenderer(new ImageListViewRenderers.DefaultRenderer());
            imageListView1.SortColumn = 0;
            imageListView1.SortOrder = SortOrder.AscendingNatural;

            imageListView1.Columns.Add(ColumnType.Name);
            imageListView1.Columns.Add(ColumnType.FileType);
            imageListView1.Columns.Add(ColumnType.FileSize);
            imageListView1.Columns.Add(ColumnType.Custom, "Taken/Modified"); // subitem 0
            imageListView1.Columns.Add(ColumnType.FilePath);
            imageListView1.Columns.Add(ColumnType.Custom, "Tag");            // subitem 1
            imageListView1.Columns.Add(ColumnType.Custom, "Target");         // subitem 2
            imageListView1.Columns.Add(ColumnType.Custom, "New Path");       // subitem 3
            imageListView1.Columns.Add(ColumnType.Custom, "Album");          // subitem 4

            // flickr Setup the background worker
            // Instantiate BackgroundWorker and attach handlers to its 
            // DowWork and RunWorkerCompleted events.
            m_flickr = new Flickr(m_ApiKey, m_SharedSecret);
        }

        private void HormigasForm_Load(object sender, EventArgs e)
        {
            if (String.Compare(m_strCulture, "en-US") == 0)
                englishToolStripMenuItem.Checked = true;
            if (String.Compare(m_strCulture, "es-ES") == 0)
                spanishToolStripMenuItem.Checked = true;
            UpdateLanguage();
            tvMain_Load(null);
            LoadHormigaObjects();
            HormigaObject ho = null;
            if (m_dObjects.TryGetValue(m_defaultFormatKey, out ho))
                m_default_format = ho.m_format;
            ho = null;
            if (m_dObjects.TryGetValue(m_defaultAlbumKey, out ho))
                m_default_album = ho.m_format;
            this.Text = m_rm.GetString("HORMIGAS_APP_NAME") + " - " + m_objectsFile;
            
        }

        public string getDefaultFormat()
        {
            return m_default_format;
        }

        public string getDefaultAlbum()
        {
            return m_default_album;
        }

        #endregion

        #region Languages
        public static ResourceManager RM
        {
            get
            {
                return m_rm;
            }
        }

        private void SetCulture()
        {
            CultureInfo objCI = new CultureInfo(m_strCulture);
            System.Threading.Thread.CurrentThread.CurrentCulture = objCI;
            System.Threading.Thread.CurrentThread.CurrentUICulture = objCI;

        }
        private void SetResource()
        {
            m_rm = ResourceManager.CreateFileBasedResourceManager("Strings", m_strResourcesPath, null);
        }

        private void UpdateLanguage()
        {
            try
            {
                SetCulture();
                SetResource();
                SetUIChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnChangedLanguage()
        {
            if (englishToolStripMenuItem.Checked)
                m_strCulture = "en-US";
            if (spanishToolStripMenuItem.Checked)
                m_strCulture = "es-ES";
            UpdateLanguage();
        }

        private void SetUIChanges()
        {
            fileToolStripDropDownButton.Text = m_rm.GetString("FILE_MENU");
            loadToolStripMenuItem.Text = m_rm.GetString("FILE_LOAD");
            saveToolStripMenuItem.Text = m_rm.GetString("FILE_SAVE");
            subfoldersToolStripButton.Text = m_rm.GetString("SUBFOLDERS");
            subfoldersToolStripButton.ToolTipText = m_rm.GetString("SUBFOLDERS");
            languagesToolStripDropDownButton.Text = m_rm.GetString("LANGUAGES");
            toolStripDropDownButtonDuplicates.Text = m_rm.GetString("DUPLICATES");
            compareSizeToolStripMenuItem.Text = m_rm.GetString("COMPARE_SIZE");
            compareCRCToolStripMenuItem.Text = m_rm.GetString("COMPARE_CRC");
            checkDuplicatesToolStripMenuItem.Text = m_rm.GetString("CHECK_DUPLICATES");
            toolStripDropDownButtonSelectedItems.Text = m_rm.GetString("SEL_ITEMS");
            setTagToolStripMenuItem.Text = m_rm.GetString("SET_TAG");
            setNewPathToolStripMenuItem.Text = m_rm.GetString("SET_TARGET");
            clearTargetToolStripMenuItem.Text = m_rm.GetString("CLEAR_TARGET");
            removeCheckedFilesToolStripMenuItem.Text = m_rm.GetString("REMOVE_CHECKED_FILES");
            excludeCheckedFilesToolStripMenuItem.Text = m_rm.GetString("EXCLUDE_CHECKED_FILES");
            copySelectedToTargetToolStripMenuItem.Text = m_rm.GetString("COPY_TO_TARGET");
            thumbnailsToolStripButton.Text = m_rm.GetString("THUMBNAILS");
            galleryToolStripButton.Text = m_rm.GetString("GALLERY");
            paneToolStripButton.Text = m_rm.GetString("PANE");
            detailsToolStripButton.Text = m_rm.GetString("DETAILS");
            englishToolStripMenuItem.Text = m_rm.GetString("LANG_EN");
            spanishToolStripMenuItem.Text = m_rm.GetString("LANG_ES");
            thumbnailSizeToolStripDropDownButton.Text = m_rm.GetString("THUMBNAIL_SIZE");
            authenticateToolStripMenuItem.Text = m_rm.GetString("AUTHENTICATE");
            completeAuthenticationToolStripMenuItem.Text = m_rm.GetString("COMPLETE_AUTHENTICATION");
            uploadFolderToolStripMenuItem.Text = m_rm.GetString("UPLOAD_SELECTED_FILES");
            clearThumbsToolStripButton.Text = m_rm.GetString("CLEAR_THUMBNAIL_CACHE");
            setFlickrsAlbumToolStripMenuItem.Text = m_rm.GetString("SET_FLICKR_ALBUM");
            setFormatToolStripMenuItem.Text = m_rm.GetString("SET_FORMAT");

            imageListView1.Columns[0].Text = m_rm.GetString("NAME");
            imageListView1.Columns[1].Text = m_rm.GetString("FILE_TYPE");
            imageListView1.Columns[2].Text = m_rm.GetString("FILE_SIZE");
            imageListView1.Columns[3].Text = m_rm.GetString("TAKEN_MODIFIED");
            imageListView1.Columns[4].Text = m_rm.GetString("FILE_PATH");
            imageListView1.Columns[5].Text = m_rm.GetString("TAG");
            imageListView1.Columns[6].Text = m_rm.GetString("TARGET");
            imageListView1.Columns[7].Text = m_rm.GetString("NEW_PATH");
            imageListView1.Columns[8].Text = m_rm.GetString("FLICKR_ALBUM");
        }
        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!englishToolStripMenuItem.Checked)
            {
                englishToolStripMenuItem.Checked = !englishToolStripMenuItem.Checked;
                spanishToolStripMenuItem.Checked = !spanishToolStripMenuItem.Checked;
                OnChangedLanguage();
            }
        }
        private void spanishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!spanishToolStripMenuItem.Checked)
            {
                spanishToolStripMenuItem.Checked = !spanishToolStripMenuItem.Checked;
                englishToolStripMenuItem.Checked = !englishToolStripMenuItem.Checked;
                OnChangedLanguage();
            }
        }

        #endregion

        #region Update UI while idle
        void Application_Idle(object sender, EventArgs e)
        {
            detailsToolStripButton.Checked = (imageListView1.View == View.Details);
            thumbnailsToolStripButton.Checked = (imageListView1.View == View.Thumbnails);
            galleryToolStripButton.Checked = (imageListView1.View == View.Gallery);
            paneToolStripButton.Checked = (imageListView1.View == View.Pane);

            integralScrollToolStripMenuItem.Checked = imageListView1.IntegralScroll;

            showCheckboxesToolStripMenuItem.Checked = imageListView1.ShowCheckBoxes;
            showFileIconsToolStripMenuItem.Checked = imageListView1.ShowFileIcons;

            x96ToolStripMenuItem.Checked = imageListView1.ThumbnailSize == new System.Drawing.Size(96, 96);
            x120ToolStripMenuItem.Checked = imageListView1.ThumbnailSize == new System.Drawing.Size(120, 120);
            x200ToolStripMenuItem.Checked = imageListView1.ThumbnailSize == new System.Drawing.Size(200, 200);

            allowCheckBoxClickToolStripMenuItem.Checked = imageListView1.AllowCheckBoxClick;
            allowColumnClickToolStripMenuItem.Checked = imageListView1.AllowColumnClick;
            allowColumnResizeToolStripMenuItem.Checked = imageListView1.AllowColumnResize;
            allowPaneResizeToolStripMenuItem.Checked = imageListView1.AllowPaneResize;
            multiSelectToolStripMenuItem.Checked = imageListView1.MultiSelect;
            allowDragToolStripMenuItem.Checked = imageListView1.AllowDrag;
            allowDropToolStripMenuItem.Checked = imageListView1.AllowDrop;
            allowDuplicateFilenamesToolStripMenuItem.Checked = imageListView1.AllowDuplicateFileNames;
            continuousCacheModeToolStripMenuItem.Checked = (imageListView1.CacheMode == CacheMode.Continuous);

            ContentAlignment ca = imageListView1.CheckBoxAlignment;
            foreach (ToolStripMenuItem item in checkboxAlignmentToolStripMenuItem.DropDownItems)
                item.Checked = (ContentAlignment)item.Tag == ca;
            ContentAlignment ia = imageListView1.IconAlignment;
            foreach (ToolStripMenuItem item in iconAlignmentToolStripMenuItem.DropDownItems)
                item.Checked = (ContentAlignment)item.Tag == ia;

            toolStripStatusLabel1.Text = string.Format("{0} Items: {1} Selected, {2} Checked",
                imageListView1.Items.Count, imageListView1.SelectedItems.Count, imageListView1.CheckedItems.Count);

            groupAscendingToolStripMenuItem.Checked = imageListView1.GroupOrder == SortOrder.Ascending;
            groupDescendingToolStripMenuItem.Checked = imageListView1.GroupOrder == SortOrder.Descending;
            sortAscendingToolStripMenuItem.Checked = imageListView1.SortOrder == SortOrder.Ascending;
            sortDescendingToolStripMenuItem.Checked = imageListView1.SortOrder == SortOrder.Descending;
        }
        #endregion

        #region Set ImageListView options
        private void checkboxAlignmentToolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ContentAlignment aligment = (ContentAlignment)item.Tag;
            imageListView1.CheckBoxAlignment = aligment;
        }

        private void iconAlignmentToolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ContentAlignment aligment = (ContentAlignment)item.Tag;
            imageListView1.IconAlignment = aligment;
        }

        private void renderertoolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ImageListView));
            RendererComboBoxItem item = (RendererComboBoxItem)renderertoolStripComboBox.SelectedItem;
            ImageListView.ImageListViewRenderer renderer = (ImageListView.ImageListViewRenderer)assembly.CreateInstance(item.FullName);
            if (renderer == null)
            {
                assembly = Assembly.GetExecutingAssembly();
                renderer = (ImageListView.ImageListViewRenderer)assembly.CreateInstance(item.FullName);
            }
            colorToolStripComboBox.Enabled = renderer.CanApplyColors;
            imageListView1.SetRenderer(renderer);
            imageListView1.Focus();
        }

        private void colorToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PropertyInfo field = ((ColorComboBoxItem)colorToolStripComboBox.SelectedItem).Field;
            ImageListViewColor color = (ImageListViewColor)field.GetValue(null, null);
            imageListView1.Colors = color;
        }

        private void detailsToolStripButton_Click(object sender, EventArgs e)
        {
            imageListView1.View = View.Details;
        }

        private void thumbnailsToolStripButton_Click(object sender, EventArgs e)
        {
            imageListView1.View = View.Thumbnails;
        }

        private void galleryToolStripButton_Click(object sender, EventArgs e)
        {
            imageListView1.View = View.Gallery;
        }

        private void paneToolStripButton_Click(object sender, EventArgs e)
        {
            imageListView1.View = View.Pane;
        }

        private void clearThumbsToolStripButton_Click(object sender, EventArgs e)
        {
            imageListView1.ClearThumbnailCache();
        }

        private void x96ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.ThumbnailSize = new System.Drawing.Size(96, 96);
        }

        private void x120ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.ThumbnailSize = new System.Drawing.Size(120, 120);
        }

        private void x200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.ThumbnailSize = new System.Drawing.Size(200, 200);
        }

        private void showCheckboxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.ShowCheckBoxes = !imageListView1.ShowCheckBoxes;
        }

        private void showFileIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.ShowFileIcons = !imageListView1.ShowFileIcons;
        }

        private void allowCheckBoxClickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowCheckBoxClick = !imageListView1.AllowCheckBoxClick;
        }

        private void allowColumnClickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowColumnClick = !imageListView1.AllowColumnClick;
        }

        private void allowColumnResizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowColumnResize = !imageListView1.AllowColumnResize;
        }

        private void allowPaneResizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowPaneResize = !imageListView1.AllowPaneResize;
        }

        private void multiSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.MultiSelect = !imageListView1.MultiSelect;
        }

        private void allowDragToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowDrag = !imageListView1.AllowDrag;
        }

        private void allowDropToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowDrop = !imageListView1.AllowDrop;
        }

        private void allowDuplicateFilenamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.AllowDuplicateFileNames = !imageListView1.AllowDuplicateFileNames;
        }

        private void continuousCacheModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageListView1.CacheMode == CacheMode.Continuous)
                imageListView1.CacheMode = CacheMode.OnDemand;
            else
                imageListView1.CacheMode = CacheMode.Continuous;
        }

        private void integralScrollToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.IntegralScroll = !imageListView1.IntegralScroll;
        }

        private void imageListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if ((e.Buttons & MouseButtons.Right) != MouseButtons.None)
            {
                // Group menu
                for (int j = groupByToolStripMenuItem.DropDownItems.Count - 1; j >= 0; j--)
                {
                    if (groupByToolStripMenuItem.DropDownItems[j].Tag != null)
                        groupByToolStripMenuItem.DropDownItems.RemoveAt(j);
                }
                int i = 0;
                foreach (ImageListView.ImageListViewColumnHeader col in imageListView1.Columns)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(col.Text);
                    item.Checked = (imageListView1.GroupColumn == i);
                    item.Tag = i;
                    item.Click += new EventHandler(groupColumnMenuItem_Click);
                    groupByToolStripMenuItem.DropDownItems.Insert(i, item);
                    i++;
                }
                if (i == 0)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem("None");
                    item.Enabled = false;
                    groupByToolStripMenuItem.DropDownItems.Insert(0, item);
                }

                // Sort menu
                for (int j = sortByToolStripMenuItem.DropDownItems.Count - 1; j >= 0; j--)
                {
                    if (sortByToolStripMenuItem.DropDownItems[j].Tag != null)
                        sortByToolStripMenuItem.DropDownItems.RemoveAt(j);
                }
                i = 0;
                foreach (ImageListView.ImageListViewColumnHeader col in imageListView1.Columns)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(col.Text);
                    item.Checked = (imageListView1.SortColumn == i);
                    item.Tag = i;
                    item.Click += new EventHandler(sortColumnMenuItem_Click);
                    sortByToolStripMenuItem.DropDownItems.Insert(i, item);
                    i++;
                }
                if (i == 0)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem("None");
                    item.Enabled = false;
                    sortByToolStripMenuItem.DropDownItems.Insert(0, item);
                }

                // Show menu
                columnContextMenu.Show(imageListView1, e.Location);
            }
        }

        private void groupColumnMenuItem_Click(object sender, EventArgs e)
        {
            int i = (int)((ToolStripMenuItem)sender).Tag;
            imageListView1.GroupColumn = i;
        }

        private void sortColumnMenuItem_Click(object sender, EventArgs e)
        {
            int i = (int)((ToolStripMenuItem)sender).Tag;
            imageListView1.SortColumn = i;
        }

        private void groupAscendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.GroupOrder = SortOrder.Ascending;
        }

        private void sortAscendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.SortOrder = SortOrder.Ascending;
        }

        private void groupDescendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.GroupOrder = SortOrder.Descending;
        }

        private void sortDescendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageListView1.SortOrder = SortOrder.Descending;
        }
        private void imageListView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                toolStripDropDownButtonSelectedItems.ShowDropDown();
                //MessageBox.Show("SI");
            }
        }

        void imageListView1_DoubleClick(object sender, System.EventArgs e)
        {
            //throw new System.NotImplementedException();
            ImageListViewItem item = ((ImageListView)sender).SelectedItems.Count > 0 ? ((ImageListView)sender).SelectedItems[0]: null;
            if (item != null)
            {
                System.Diagnostics.Process.Start(item.FileName);
            }
        }
        

        #endregion

        #region Set selected image to PropertyGrid
        private void imageListView1_SelectionChanged(object sender, EventArgs e)
        {
            ImageListViewItem sel = null;
            if (imageListView1.SelectedItems.Count > 0)
                sel = imageListView1.SelectedItems[0];
            propertyGrid1.SelectedObject = sel;
        }
        #endregion

        #region Change Selection/Checkboxes
        private void imageListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.A)
                    imageListView1.SelectAll();
                else if (e.KeyCode == Keys.U)
                    imageListView1.ClearSelection();
                else if (e.KeyCode == Keys.I)
                    imageListView1.InvertSelection();
            }
            else if (e.Alt)
            {
                if (e.KeyCode == Keys.A)
                    imageListView1.CheckAll();
                else if (e.KeyCode == Keys.U)
                    imageListView1.UncheckAll();
                else if (e.KeyCode == Keys.I)
                    imageListView1.InvertCheckState();
            }
        }
        #endregion

        #region Update folder list asynchronously

        private void ProcessFile(FileInfo p)
        {
            string fullName = p.FullName;
            imageListView1.Items.Add(fullName);
            ImageListViewItem item = imageListView1.Items[imageListView1.Items.Count - 1];
            DateTime dt = DateTime.Now;
            if (getDateToCompare(item, ref dt))
                item.SetSubItemText(0, composeDateAsString(dt));
            HormigaObject ho = null;
            if (m_dObjects.TryGetValue(fullName, out ho))
            {
                item.SetSubItemText(1, ho.m_tag);
                item.SetSubItemText(2, ho.m_target);
                item.SetSubItemText(3, composeNewFilePathName(item));
                item.SetSubItemText(4, composeNewAlbumName(item));
            }
        }
        private void ApplyAllFiles(DirectoryInfo dirInfo, Action<FileInfo> fileAction)
        {
            // Display wait cursor while
            Cursor cursor = imageListView1.Cursor;
            imageListView1.Cursor = Cursors.WaitCursor;

            foreach (FileInfo p in dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                if (p.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".cur", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".emf", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".wmf", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".mpeg", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    fileAction(p);
                }
            }
            // Restore previous cursor
            imageListView1.Cursor = cursor;
        }

        private void PopulateListView()
        {
            m_populatingView = true;
            imageListView1.Items.Clear();
            imageListView1.SuspendLayout();

            foreach (string pathFolder in m_lDirsToShow)
            {
                DirectoryInfo di = new DirectoryInfo(pathFolder);
                ApplyAllFiles(di, ProcessFile);
            }

            imageListView1.ResumeLayout();
            m_populatingView = false;
        }

        private void PopulateListView(DirectoryInfo path)
        {
            imageListView1.Items.Clear();
            imageListView1.SuspendLayout();
            int i = 0;
            foreach (FileInfo p in path.GetFiles("*.*"))
            {
                if (p.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".cur", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".emf", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".wmf", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    imageListView1.Items.Add(p.FullName);
                    if (i == 1) imageListView1.Items[imageListView1.Items.Count - 1].Enabled = false;
                    i++;
                    if (i == 3) i = 0;
                }
            }
            imageListView1.ResumeLayout();
        }

        //private void tvMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        //{
        //    TreeNode node = e.Node;
        //    KeyValuePair<DirectoryInfo, bool> ktag = (KeyValuePair<DirectoryInfo, bool>)node.Tag;
        //    if (ktag.Value == true)
        //        return;
        //    node.Nodes.Clear();
        //    node.Nodes.Add("", "Loading...", 3, 3);
        //    while (bw.IsBusy) ;
        //    bw.RunWorkerAsync(node);
        //}

        //private void tvMain_AfterSelect(object sender, TreeViewEventArgs e)
        //{
        //    if (e.Node.Tag == null) return;
        //    KeyValuePair<DirectoryInfo, bool> ktag = (KeyValuePair<DirectoryInfo, bool>)e.Node.Tag;
        //    PopulateListView(ktag.Key);
        //}

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            KeyValuePair<TreeNode, List<TreeNode>> kv = (KeyValuePair<TreeNode, List<TreeNode>>)e.Result;
            TreeNode rootNode = kv.Key;
            List<TreeNode> nodes = kv.Value;
            if (rootNode.Tag == null)
            {
                tvMain.Nodes.Clear();
                foreach (TreeNode node in nodes)
                    tvMain.Nodes.Add(node);
            }
            else
            {
                KeyValuePair<DirectoryInfo, bool> ktag = (KeyValuePair<DirectoryInfo, bool>)rootNode.Tag;
                rootNode.Tag = new KeyValuePair<DirectoryInfo, bool>(ktag.Key, true);
                rootNode.Nodes.Clear();
                foreach (TreeNode node in nodes)
                    rootNode.Nodes.Add(node);
            }
        }

        private static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            TreeNode rootNode = e.Argument as TreeNode;

            List<TreeNode> nodes = GetNodes(rootNode);

            e.Result = new KeyValuePair<TreeNode, List<TreeNode>>(rootNode, nodes);
        }

        private static List<TreeNode> GetNodes(TreeNode rootNode)
        {
            if (rootNode.Tag == null)
            {
                List<TreeNode> volNodes = new List<TreeNode>();
                foreach (DriveInfo info in System.IO.DriveInfo.GetDrives())
                {
                    if (info.IsReady)
                    {
                        DirectoryInfo rootPath = info.RootDirectory;
                        TreeNode volNode = new TreeNode(info.VolumeLabel + " (" + info.Name + ")", 0, 0);
                        volNode.Tag = new KeyValuePair<DirectoryInfo, bool>(rootPath, false);
                        List<TreeNode> nodes = GetNodes(volNode);
                        volNode.Tag = new KeyValuePair<DirectoryInfo, bool>(rootPath, true);
                        volNode.Nodes.Clear();
                        foreach (TreeNode node in nodes)
                            volNode.Nodes.Add(node);

                        volNode.Expand();
                        volNodes.Add(volNode);
                    }
                }

                return volNodes;
            }
            else
            {
                KeyValuePair<DirectoryInfo, bool> kv = (KeyValuePair<DirectoryInfo, bool>)rootNode.Tag;
                bool done = kv.Value;
                if (done)
                    return new List<TreeNode>();

                DirectoryInfo rootPath = kv.Key;
                List<TreeNode> nodes = new List<TreeNode>();

                DirectoryInfo[] dirs = new DirectoryInfo[0];
                try
                {
                    dirs = rootPath.GetDirectories();
                }
                catch
                {
                    return new List<TreeNode>();
                }
                foreach (DirectoryInfo info in dirs)
                {
                    if ((info.Attributes & FileAttributes.System) != FileAttributes.System)
                    {
                        TreeNode aNode = new TreeNode(info.Name, 1, 2);
                        aNode.Tag = new KeyValuePair<DirectoryInfo, bool>(info, false);
                        GetDirectories(aNode);
                        nodes.Add(aNode);
                    }
                }
                return nodes;
            }
        }

        private static void GetDirectories(TreeNode node)
        {
            KeyValuePair<DirectoryInfo, bool> ktag = (KeyValuePair<DirectoryInfo, bool>)node.Tag;
            DirectoryInfo rootPath = ktag.Key;

            DirectoryInfo[] dirs = new DirectoryInfo[0];
            try
            {
                dirs = rootPath.GetDirectories();
            }
            catch
            {
                return;
            }
            foreach (DirectoryInfo info in dirs)
            {
                if ((info.Attributes & FileAttributes.System) != FileAttributes.System)
                {
                    TreeNode aNode = new TreeNode(info.Name, 1, 2);
                    aNode.Tag = new KeyValuePair<DirectoryInfo, bool>(info, false);
                    if (GetDirCount(info) != 0)
                    {
                        aNode.Nodes.Add("Dummy1");
                    }
                    node.Nodes.Add(aNode);
                }
            }
            node.Tag = new KeyValuePair<DirectoryInfo, bool>(ktag.Key, true);
        }

        private static int GetDirCount(DirectoryInfo rootPath)
        {
            DirectoryInfo[] dirs = new DirectoryInfo[0];
            try
            {
                dirs = rootPath.GetDirectories();
            }
            catch
            {
                return 0;
            }

            return dirs.Length;
        }
        #endregion

        #region Subfolders

        private void repopulateView(TreeNode node)
        {
            if (node == null || node.Tag == null) return;
            while (m_populatingView) ;
            PopulateListView();
        }

        private void subfoldersStripButton_Click(object sender, EventArgs e)
        {
            subfoldersToolStripButton.Checked = !subfoldersToolStripButton.Checked;
            repopulateView(tvMain.SelectedNode);
        }
        #endregion

        #region Duplicates
        public bool getDateToCompare(ImageListViewItem item, ref DateTime dt)
        {
            if (item == null)
                return false;
            dt = item.DateTaken;
            if (dt.Year != 1)
                return true;
            dt = item.DateModified;
            return true;
        }
        private void compareSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compareSizeToolStripMenuItem.Checked = !compareSizeToolStripMenuItem.Checked;
            if (!compareSizeToolStripMenuItem.Checked)
                compareCRCToolStripMenuItem.Checked = false;
        }

        private void compareCRCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compareSizeToolStripMenuItem.Checked = true;
            compareCRCToolStripMenuItem.Checked = !compareCRCToolStripMenuItem.Checked;
        }

        private void checkDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkDuplicatesByDateTakenModified();
        }

        private void checkDuplicatesByDateTakenModified()
        {
            imageListView1.SortColumn = 3; // date taken / modified
            imageListView1.SortOrder = SortOrder.Ascending;
            imageListView1.Sort();
            // recorremos la lista y comparamos por datetaken y/o tamaño y/o crc
            ImageListViewItem item = null, prevItem = null;
            for (int i = 0; i < imageListView1.Items.Count; i++)
            {
                item = imageListView1.Items[i];
                prevItem = i > 0 ? imageListView1.Items[i - 1] : null;

                if (prevItem != null)
                {
                    DateTime dateTaken = item.DateTaken;
                    DateTime prevDateTaken = prevItem.DateTaken;
                    if (getDateToCompare(item, ref dateTaken) && getDateToCompare(prevItem, ref prevDateTaken))
                    {
                        bool isDup = dateTaken == prevDateTaken;

                        // Size
                        if (isDup && compareSizeToolStripMenuItem.Checked)
                        {
                            long size = item.FileSize;
                            long prevSize = item.FileSize;
                            isDup = size == prevSize;
                        }
                        // CRC
                        if (isDup && compareCRCToolStripMenuItem.Checked)
                        {
                            var md5 = MD5.Create();
                            var stream = File.OpenRead(item.FileName);
                            var prevStream = File.OpenRead(prevItem.FileName);
                            if (stream != null && prevStream != null)
                            {
                                byte[] b1 = md5.ComputeHash(stream);
                                byte[] b2 = md5.ComputeHash(prevStream);
                                if (b1 != b2)
                                {
                                    if (b1 == null || b2 == null)
                                        isDup = false;
                                    else if (b1.Length != b2.Length)
                                        isDup = false;
                                    if (isDup)
                                    {
                                        for (int j = 0; j < b1.Length; j++)
                                        {
                                            if (b1[j] != b2[j])
                                            {
                                                isDup = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        item.Checked = isDup;
                    }
                }
            }
        }
        #endregion

        #region Copy Selected To
        public bool getExcludeCheckedFiles()
        {
            return excludeCheckedFilesToolStripMenuItem.Checked;
        }
        private DialogResult NewPathBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = HormigasForm.RM.GetString("OK");
            buttonCancel.Text = HormigasForm.RM.GetString("CANCEL");
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = DialogResult.OK;
            bool isNotvalid = false;
            do
            {
                dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.Cancel)
                    return dialogResult;
                value = textBox.Text;
                isNotvalid = false;
                if (value.Length > 0)
                {
                    try
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + value);
                        Directory.Delete(Directory.GetCurrentDirectory() + "\\" + value);
                    }
                    catch (Exception ex)
                    {
                        isNotvalid = true;
                        Console.WriteLine(ex.ToString());
                    }
                }
            } while (isNotvalid);
            return dialogResult;
        }
        
        private void excludeCheckedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            excludeCheckedFilesToolStripMenuItem.Checked = !excludeCheckedFilesToolStripMenuItem.Checked;
        }
        private void setNewPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (ImageListViewItem item in imageListView1.SelectedItems)
                {
                    item.SetSubItemText(2, folderBrowserDialog1.SelectedPath);
                    item.SetSubItemText(3, composeNewFilePathName(item));
                    HormigaObject ho = null;
                    if (!m_dObjects.TryGetValue(item.FileName, out ho))
                    {
                        HormigaObject newho = new HormigaObject(item.FileName,
                            folderBrowserDialog1.SelectedPath,
                            item.GetSubItemText(1),
                            item.GetSubItemText(4),
                            m_default_format,
                            item.FilePath);
                        m_dObjects.Add(item.FileName, newho);
                    }
                    else
                    {
                        ho.m_target = folderBrowserDialog1.SelectedPath;
                    }
                }

                SaveHormigaObjects();
            }
        }
        private void clearTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageListViewItem item in imageListView1.SelectedItems)
            {
                item.SetSubItemText(2, "");
                item.SetSubItemText(3, composeNewFilePathName(item));
                HormigaObject ho = null;
                if (!m_dObjects.TryGetValue(item.FileName, out ho))
                {
                    HormigaObject newho = new HormigaObject(item.FileName,
                        "", 
                        item.GetSubItemText(1),
                        item.GetSubItemText(4),
                        m_default_format,
                        item.FilePath);
                    m_dObjects.Add(item.FileName, newho);
                }
                else
                {
                    ho.m_target = "";
                }
            }
            SaveHormigaObjects();
        }
        private void setFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldFormat = "";
            ImageListViewItem it = imageListView1.SelectedItems.Count == 1 ? imageListView1.SelectedItems[0] : null;
            HormigaObject ho = null;
            if (it != null && m_dObjects.TryGetValue(it.FileName, out ho))
                oldFormat = ho.m_format;
            else
                oldFormat = m_default_format;
            NewFormatForm formatForm = new NewFormatForm(this,it, oldFormat);
            if (formatForm.ShowDialog() == DialogResult.OK)
            {
                string format = formatForm.ReturnFormat;
                if (formatForm.ReturnDefault)
                {
                    m_default_format = format;
                    ho = null;
                    if (!m_dObjects.TryGetValue(m_defaultFormatKey, out ho))
                    {
                        HormigaObject newho = new HormigaObject(m_defaultFormatKey, "", "", "", m_default_format, "");
                        m_dObjects.Add(m_defaultFormatKey, newho);
                    }
                    else
                        ho.m_format = m_default_format;
                }
                foreach (ImageListViewItem item in imageListView1.SelectedItems)
                {
                    ho = null;
                    if (!m_dObjects.TryGetValue(item.FileName, out ho))
                    {
                        HormigaObject newho = new HormigaObject(item.FileName,
                            item.GetSubItemText(2),
                            item.GetSubItemText(1), 
                            item.GetSubItemText(4),
                            format,
                            item.FilePath);
                        m_dObjects.Add(item.FileName, newho);
                    }
                    else if (ho.m_format != format)
                        ho.m_format = format;
                    string newFilePathName = composeNewFilePathName(item);
                    item.SetSubItemText(3, newFilePathName);
                }
                SaveHormigaObjects();
            }
        }
        private void setTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newPath = "";
            if (NewPathBox(m_rm.GetString("TAG"), m_rm.GetString("NEW_TAG_NAME"), ref newPath) == DialogResult.OK)
            {
                foreach (ImageListViewItem item in imageListView1.SelectedItems)
                {
                    item.SetSubItemText(1, newPath);
                    item.SetSubItemText(3, composeNewFilePathName(item));
                    string newAlbum = composeNewAlbumName(item);
                    item.SetSubItemText(4, newAlbum);
                    HormigaObject ho = null;
                    if (!m_dObjects.TryGetValue(item.FileName, out ho))
                    {
                        HormigaObject newho = new HormigaObject(item.FileName, 
                            item.GetSubItemText(2), 
                            newPath, 
                            item.GetSubItemText(4),
                            m_default_format,
                            item.FilePath);
                        m_dObjects.Add(item.FileName, newho);
                    }
                    else
                    {
                        ho.m_tag = newPath;
                        ho.m_album = newAlbum;
                    }
                }
                SaveHormigaObjects();
            }
        }
        public string getDuplicatedName(string path)
        {
            int n = 1;
            while (File.Exists(path))
            {
                int idxExt = path.LastIndexOf(".");
                string newName = path.Substring(0, idxExt);
                newName += "_" + n.ToString() + path.Substring(idxExt);
                n++;
                path = newName;
            }
            return path;
        }
        private string composeDateAsString(DateTime dt)
        {
            if (dt == null || dt.Year == 1)
                return "";
            string value = dt.Year.ToString() + "_" + 
                           dt.Month.ToString("D2") + "_" +
                           dt.Day.ToString("D2") + "_" +
                           dt.Hour.ToString("D2") +
                           dt.Minute.ToString("D2") +
                           dt.Second.ToString("D2") +
                           dt.Millisecond.ToString("D3") ;
            return value;

        }
        public string parseFormat(ImageListViewItem item)
        {
            string format = "";
            HormigaObject ho = null;
            if (!m_dObjects.TryGetValue(item.FileName, out ho))
                format = m_default_format;
            else
                format = ho.m_format;
            DateTime dt = DateTime.Now;
            if (getDateToCompare(item, ref dt))
                return parseFormat(format,
                    item.Text,
                    item.GetSubItemText(1),
                    dt.Year.ToString(),
                    dt.Month.ToString("D2"),
                    dt.Day.ToString("D2"),
                    dt.Hour.ToString("D2") +
                    dt.Minute.ToString("D2") +
                    dt.Second.ToString("D2") +
                    dt.Millisecond.ToString("D3"));
            return "";
        }
        public string parseAlbum(ImageListViewItem item)
        {
            string format = "";
            HormigaObject ho = null;
            if (!m_dObjects.TryGetValue(item.FileName, out ho))
                format = m_default_album;
            else
                format = ho.m_album;
            DateTime dt = DateTime.Now;
            if (getDateToCompare(item, ref dt))
                return parseFormat(format,
                    item.Text,
                    item.GetSubItemText(1),
                    dt.Year.ToString(),
                    dt.Month.ToString("D2"),
                    dt.Day.ToString("D2"),
                    dt.Hour.ToString("D2") +
                    dt.Minute.ToString("D2") +
                    dt.Second.ToString("D2") +
                    dt.Millisecond.ToString("D3"),false);
            return "";
        }
        public string parseFormat(string txt, string fileName, string tag, string year, string month, string day, string time, bool addExtension=true)
        {
            string extension = Path.GetExtension(fileName);
            if (addExtension && extension.Length > 0)
                fileName = fileName.Replace(extension, "");
            string newName = "";
            string invalidPathChars = new string(Path.GetInvalidPathChars());
            string invalidFileChars = new string(Path.GetInvalidFileNameChars());
            for (int i = 0; i < txt.Length; i++)
            {
                char c = txt[i];
                if (c != '\\' && c != '%')
                {
                    if (invalidPathChars.Contains(c.ToString()) || invalidFileChars.Contains(c.ToString()))
                        continue;
                    newName += c.ToString();
                }
                else if (c == '\\')
                    newName += c.ToString();
                else if (c == '%')
                {
                    if (i + 1 < txt.Length)
                    {
                        if (txt[i + 1] == 'N' || txt[i + 1] == 'n')
                        {
                            newName += fileName;
                            i++;
                        }
                        else if (txt[i + 1] == 'E' || txt[i + 1] == 'e')
                        {
                            newName += tag;
                            i++;
                        }
                        else if (txt[i + 1] == 'A' || txt[i + 1] == 'a')
                        {
                            newName += year;
                            i++;
                        }
                        else if (txt[i + 1] == 'M' || txt[i + 1] == 'm')
                        {
                            newName += month;
                            i++;
                        }
                        else if (txt[i + 1] == 'D' || txt[i + 1] == 'd')
                        {
                            newName += day;
                            i++;
                        }
                        else if (txt[i + 1] == 'H' || txt[i + 1] == 'h')
                        {
                            newName += time;
                            i++;
                        }
                    }
                }
            }
            if (addExtension && newName.Length > 0 && newName[newName.Length - 1] != '\\')
                newName += extension;
            return newName;
        }
        public string composeNewFileName(ImageListViewItem item)
        {
            string nameByFormat = parseFormat(item);

            // quitamos el path que hubiera
            int idx = nameByFormat.LastIndexOf('\\');
            if (idx != -1)
                nameByFormat = nameByFormat.Substring(idx);
            return nameByFormat;
            //DateTime dt = DateTime.Now;
            //if (!getDateToCompare(item, ref dt))
            //    return "";
            //string newFileName = dt.Year.ToString() + "_" + dt.Month.ToString("D2") + "_" + dt.Day.ToString("D2");
            //string extension = Path.GetExtension(item.FileName);

            //string tag = item.GetSubItemText(1);
            //if (tag.Length > 0)
            //    newFileName += "_" + tag;
            //newFileName += "_" +  dt.Hour.ToString("D2") +
            //       dt.Minute.ToString("D2") +
            //       dt.Second.ToString("D2") +
            //       dt.Millisecond.ToString("D2") +
            //       extension;
            //return newFileName;
        }
        public string composeNewFilePathName(ImageListViewItem item)
        {
            // Si no tiene target -> nada
            string path = item.GetSubItemText(2);
            if (path.Length == 0)
                return "";
            string nameByFormat = parseFormat(item);
            return path + nameByFormat;

            //DateTime dt = DateTime.Now;
            //if (!getDateToCompare(item, ref dt))
            //    return "";
            //// Si se quiere directorios por YYYY/MM
            //if (prependFoldersByDateToolStripMenuItem.Checked)
            //{
            //    string yyyy = dt.Year.ToString();
            //    path = Path.Combine(path, yyyy);
            //    string mm = dt.Month.ToString("D2");
            //    path = Path.Combine(path, yyyy + "_" + mm);
            //}

            //string newFileName = composeNewFileName(item);
            //string tag = item.GetSubItemText(1);
            //if (tag.Length > 0)
            //    path = path + "_" + tag; // same level MONTH

            //path = Path.Combine(path, newFileName);

            //return path;
        }
        private string composeNewAlbumName(ImageListViewItem item)
        {
            string nameByAlbum = parseAlbum(item);

            // quitamos el path que hubiera
            int idx = nameByAlbum.LastIndexOf('\\');
            if (idx != -1)
                nameByAlbum = nameByAlbum.Substring(idx);
            return nameByAlbum;

            //DateTime dt = DateTime.Now;
            //if (!getDateToCompare(item, ref dt))
            //    return "";
            //string album = parseFormat("%a_%m_%e",
            //    "",
            //    item.GetSubItemText(1),
            //    dt.Year.ToString(),
            //    dt.Month.ToString("D2"),
            //    "",
            //    "");

            //return album;
            //string album = "";
            //DateTime dt = DateTime.Now;
            //if (!getDateToCompare(item, ref dt))
            //    return "";
            //// Si se quiere directorios por YYYY/MM
            //string yyyy = dt.Year.ToString();
            //string mm = dt.Month.ToString("D2");
            //album = yyyy + "_" + mm;
            //string tag = item.GetSubItemText(1);
            //if (tag.Length > 0)
            //    album = album + "_" + tag;

            //return album;
        }
        private void copySelectedToTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageListView.ImageListViewSelectedItemCollection col_selectedItems = imageListView1.SelectedItems;
            if (col_selectedItems.Count == 0)
                return;
            List<ImageListViewItem> list_selectedItems = new List<ImageListViewItem>();
            foreach (ImageListViewItem item in col_selectedItems)
            {
                ImageListViewItem newItem = (ImageListViewItem)item.Clone();
                list_selectedItems.Add(newItem);
            }
            BackgroundWorkerForm bwForm = new BackgroundWorkerForm(this,list_selectedItems, false);
            bwForm.Show();
        }
        private void removeCheckedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(m_rm.GetString("REMOVE_FILES_QUESTION"), m_rm.GetString("REMOVE_CHECKED_FILES"), MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                // Display wait cursor while
                Cursor cursor = imageListView1.Cursor;
                imageListView1.Cursor = Cursors.WaitCursor;
                foreach (ImageListViewItem item in imageListView1.SelectedItems)
                {
                    if (item.Checked)
                    {
                        string fileName = item.FileName;
                        try
                        {
                            File.Delete(item.FileName);
                            imageListView1.Items.Remove(item);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                // Restore previous cursor
                imageListView1.Cursor = cursor;
            }
        }
        #endregion

        #region Trees
        private void tvMain_Load(TreeNode rootNode)
        {
            if (rootNode == null)
            {
                if (tvMain.Nodes.Count == 0 || (tvMain.Nodes.Count > 0 && tvMain.Nodes[0].Name == ""))
                {
                    tvMain.Nodes.Clear();
                    // drives
                    foreach (DriveInfo info in System.IO.DriveInfo.GetDrives())
                    {
                        if (info.IsReady)
                        {
                            DirectoryInfo rootPath = info.RootDirectory;
                            TreeNode volNode = new TreeNode(info.Name, 0, 0);
                            volNode.Name = rootPath.FullName;
                            volNode.Tag = rootPath;
                            tvMain.Nodes.Add(volNode);
                            if (rootPath.GetDirectories().Length > 0)
                                volNode.Nodes.Add("Dummy");
                        }
                    }
                }
            }
            else
            {
                if (rootNode.Nodes.Count == 0 || (rootNode.Nodes.Count > 0 && rootNode.Nodes[0].Name == ""))
                {
                    rootNode.Nodes.Clear();
                    string path = rootNode.Name;
                    DirectoryInfo info = new DirectoryInfo(path);
                    if (info != null)
                    {
                        DirectoryInfo[] dirs = info.GetDirectories();
                        foreach (DirectoryInfo di in dirs)
                        {
                            if ((di.Attributes & FileAttributes.System) != FileAttributes.System)
                            {
                                TreeNode aNode = new TreeNode(di.Name, 1, 2);
                                aNode.Name = di.FullName;
                                aNode.Tag = di;
                                rootNode.Nodes.Add(aNode);
                                if (di.GetDirectories().Length > 0)
                                    aNode.Nodes.Add("Dummy");
                            }
                        }
                    }
                }
            }
        }

        void tvMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            tvMain_Load(node);
        }

        void tvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Name == "")
                return;
            if (node.Checked)
            {
                addNodeToSelected(node, null);
                if (subfoldersToolStripButton.Checked)
                {
                    foreach (TreeNode subNode in node.Nodes)
                        subNode.Checked = true;
                }
                // added -> save object
                if (!m_dObjects.ContainsKey(node.Name))
                {
                    HormigaObject obj = new HormigaObject("", "", "", "", m_default_format, node.Name);
                    m_dObjects.Add(node.Name, obj);
                }
            }
            else
            {
                removeNodeFromSelected(node);
                if (subfoldersToolStripButton.Checked)
                {
                    foreach (TreeNode subNode in node.Nodes)
                        subNode.Checked = false;
                }
                // removed -> remove objects (all that contains that key)
                var toRemove = new List<string>();
                foreach (KeyValuePair<string, HormigaObject> kvp in m_dObjects)
                {
                    if (kvp.Key.Contains(node.Name))
                        toRemove.Add(kvp.Key);
                }
                foreach (string s in toRemove)
                    m_dObjects.Remove(s);
            }

            SaveHormigaObjects();
        }

        private void tvMain_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Name == "") return;

            //subfoldersToolStripButton.Checked = false;
            //KeyValuePair<DirectoryInfo, bool> ktag = (KeyValuePair<DirectoryInfo, bool>)e.Node.Tag;
            m_lDirsToShow.Clear();
            m_lDirsToShow.Add(e.Node.Name);
            PopulateListView();
        }

        void tvSel_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            TreeNode source = (TreeNode)node.Tag;
            if (source != null && source.Checked != node.Checked)
                source.Checked = node.Checked; // set check in tvMain
        }

        void tvSel_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            m_lDirsToShow.Clear();
            dirsToShow(e.Node);
            repopulateView(e.Node);
        }

        void addNodeToSelected(TreeNode source, TreeNode child)
        {
            if (tvSel.Nodes.Count == 0)
                tvSel.Nodes.Add("All selected nodes");
            if (source == null)
            {
                if (child != null)
                {
                    int idx = tvSel.Nodes[0].Nodes.Add(child);
                    TreeNode n = tvSel.Nodes[0].Nodes[idx];
                    n.EnsureVisible();

                }
                return;
            }

            string pathFolder = source.Name;
            TreeNode[] list = tvSel.Nodes.Find(pathFolder, true);
            TreeNode newNode = null;
            if (list.Length == 0)
            {
                newNode = new TreeNode(source.Text, source.ImageIndex, source.SelectedImageIndex);
                newNode.Tag = source;
                newNode.Name = pathFolder;
            }
            else
                newNode = list[0];
            if (newNode.Checked == false && child == null)
                newNode.Checked = true;
            if (child != null)
            {
                newNode.Nodes.Add(child);
                newNode.Expand();
            }
            if (list.Length == 0)
                addNodeToSelected(source.Parent, newNode);
        }

        public TreeNode FindNodeInParents(string name, TreeNode childToFind)
        {
            bool finished = false;
            if (tvMain.Nodes.Count == 0)
                return null;
            TreeNodeCollection nodes = tvMain.Nodes;
            int idx = 0, startIdx = 0;
            while (!finished)
            {
                idx = name.IndexOf('\\', startIdx);
                if (idx > 0)
                {
                    string subName = name.Substring(0, idx + 1);
                    foreach (TreeNode source in nodes)
                    {
                        if (source.Name == name)
                            return source;
                        if (subName.Contains(source.Name))
                        {
                            tvMain_Load(source);
                            //if (!source.IsExpanded)
                            //    source.Expand();
                            //while (!source.IsExpanded) ;
                            nodes = source.Nodes;
                            break;
                        }

                    }
                    startIdx = idx + 1;
                }
                else
                {
                    foreach (TreeNode source in nodes)
                    {
                        if (source.Name == name)
                            return source;
                    }
                    finished = true;
                }
            }
            return null;
        }


        void removeNodeFromSelected(TreeNode source)
        {
            if (source == null)
                return;
            string pathFolder = source.Name;
            TreeNode[] list = tvSel.Nodes.Find(pathFolder, true);
            if (list.Length == 0)
                return;
            TreeNode found = list[0];
            TreeNode parent = found.Parent;
            if (found.Nodes.Count > 0)
            {
                if (found.Checked)
                    found.Checked = false;
                return;
            }
            else
                tvSel.Nodes.Remove(found);
            found = parent;
            while (found != null && found.Nodes.Count == 0)
            {
                parent = found.Parent;
                if (found.Checked == false)
                    tvSel.Nodes.Remove(found);
                found = parent;
            }
        }

        private void dirsToShow(TreeNode node)
        {
            if (node.Checked)
                m_lDirsToShow.Add(node.Name);

            foreach (TreeNode child in node.Nodes)
                dirsToShow(child);
        }

        private void subfoldersToolStripButton_Click(object sender, EventArgs e)
        {
            subfoldersToolStripButton.Checked = !subfoldersToolStripButton.Checked;
            subfoldersToolStripButton.CheckState = subfoldersToolStripButton.Checked ? CheckState.Checked: CheckState.Unchecked;
            repopulateView(tvMain.SelectedNode);
        }

        

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_objectsFile = openFileDialog1.FileName;
                LoadHormigaObjects();
                this.Text = m_rm.GetString("HORMIGAS_APP_NAME")+ " - " + m_objectsFile;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_objectsFile = saveFileDialog1.FileName;
            }
        }
        #endregion

        #region Persistence
        private void SaveHormigaObjects()
        {
            SortedList<string, HormigaObject> copy = new SortedList<string, HormigaObject>(m_dObjects, StringComparer.CurrentCultureIgnoreCase);

            try
            {
                using (Stream stream = File.Open(m_objectsFile, FileMode.Create))
                {
                    long s = stream.Length;
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, copy);
                }
            }
            catch (IOException)
            {
            }
        }

        private void LoadHormigaObjects()
        {
            try
            {
                tvSel.Nodes.Clear();
                m_dObjects.Clear();
                using (Stream stream = File.Open(m_objectsFile, FileMode.Open))
                {
                    if (stream.Length > 0)
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        SortedList<string, HormigaObject> list = (SortedList<string, HormigaObject>)bin.Deserialize(stream);
                        foreach (KeyValuePair<string, HormigaObject> kvp in list)
                        {
                            m_dObjects.Add(kvp.Key, kvp.Value);

                            // encontrar en tvMain el kvp.Key y añadirlo (marcarlo como checked)
                            TreeNode n = FindNodeInParents(kvp.Key, null);
                            if (n != null)
                            {
                                n.EnsureVisible();
                                n.Checked = true;
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
            }
        }
        #endregion

        #region Flickr
        public static Flickr FLICKR
        {
            get
            {
                return m_flickr;
            }
        }
        private static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private void authenticateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForInternetConnection())
            {
                MessageBox.Show( m_rm.GetString("NO_INTERNET"),"", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            m_requestToken = m_flickr.OAuthGetRequestToken("oob");
            // Calculate the URL at Flickr to redirect the user to
            string flickrUrl = m_flickr.OAuthCalculateAuthorizationUrl(m_requestToken.Token, AuthLevel.Write);
            MessageBox.Show(m_rm.GetString("AUTHENTICATE_INFO"),m_rm.GetString("AUTHENTICATE"),MessageBoxButtons.OK,MessageBoxIcon.Information);
            // The following line will load the URL in the users default browser.
            System.Diagnostics.Process.Start(flickrUrl);
            completeAuthenticationToolStripMenuItem_Click(this, null);
        }
        private void completeAuthenticationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForInternetConnection())
            {
                MessageBox.Show(m_rm.GetString("NO_INTERNET"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string verifier = "";
            if (NewPathBox(m_rm.GetString("COMPLETE_AUTHENTICATION"), m_rm.GetString("FLICKR_VERIFIER_TYPE"), ref verifier) == DialogResult.OK)
            {
                try
                {
                    // Store this Token for later usage, 
                    // or set your Flickr instance to use it.
                    m_accessToken = m_flickr.OAuthGetAccessToken(m_requestToken, verifier);
                    Console.WriteLine("User authenticated successfully");
                    Console.WriteLine("Authentication token is " + m_accessToken.Token);
                    //m_flickr.AuthToken = auth.Token;
                    Console.WriteLine("User id is " + m_accessToken.Username);
                    uploadFolderToolStripMenuItem.Enabled = true;
                    MessageBox.Show(m_rm.GetString("AUTHENTICATE_OK"), m_rm.GetString("INFORMATION"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (FlickrException ex)
                {
                    // If user did not authenticat your application 
                    // then a FlickrException will be thrown.
                    Console.WriteLine("User did not authenticate you");
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show(m_rm.GetString("AUTHENTICATE_ERROR"), m_rm.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
        private void uploadFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForInternetConnection())
            {
                MessageBox.Show(m_rm.GetString("NO_INTERNET"),"", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ImageListView.ImageListViewSelectedItemCollection col_selectedItems = imageListView1.SelectedItems;
            if (col_selectedItems.Count == 0)
                return;
            List<ImageListViewItem> list_selectedItems = new List<ImageListViewItem>();
            foreach (ImageListViewItem item in col_selectedItems)
            {
                ImageListViewItem newItem = (ImageListViewItem)item.Clone();
                list_selectedItems.Add(newItem);
            }
            BackgroundWorkerForm bwForm = new BackgroundWorkerForm(this, list_selectedItems, true);
            bwForm.Show();
            return;

        }
        public string findPhotoSetId(PhotosetCollection colAlbums, string set_title)
        {
            try
            {
                foreach (Photoset set in colAlbums)
                {
                    if (set.Title == set_title)
                        return set.PhotosetId;
                }
            }
            catch (FlickrException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return "";
        }
        private void setFlickrsAlbumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldFormat = "";
            ImageListViewItem it = imageListView1.SelectedItems.Count == 1 ? imageListView1.SelectedItems[0] : null;
            HormigaObject ho = null;
            if (it != null && m_dObjects.TryGetValue(it.FileName, out ho))
                oldFormat = ho.m_album;
            else
                oldFormat = m_default_album;
            NewFormatForm formatForm = new NewFormatForm(this, it, oldFormat, true);
            if (formatForm.ShowDialog() == DialogResult.OK)
            {
                string format = formatForm.ReturnFormat;
                if (formatForm.ReturnDefault)
                {
                    m_default_album = format;
                    ho = null;
                    if (!m_dObjects.TryGetValue(m_defaultAlbumKey, out ho))
                    {
                        HormigaObject newho = new HormigaObject(m_defaultAlbumKey, "", "", "", m_default_album, "");
                        m_dObjects.Add(m_defaultAlbumKey, newho);
                    }
                    else
                        ho.m_album = m_default_album;
                }
                foreach (ImageListViewItem item in imageListView1.SelectedItems)
                {
                    ho = null;
                    if (!m_dObjects.TryGetValue(item.FileName, out ho))
                    {
                        HormigaObject newho = new HormigaObject(item.FileName,
                            item.GetSubItemText(2),
                            item.GetSubItemText(1),
                            format,
                            m_default_format,
                            item.FilePath);
                        m_dObjects.Add(item.FileName, newho);
                    }
                    else if (ho.m_album != format)
                        ho.m_album = format;
                    string newAlbumName = composeNewAlbumName(item);
                    item.SetSubItemText(4, newAlbumName);
                }
                SaveHormigaObjects();
            }
            //string newAlbum = "";
            //if (NewPathBox(m_rm.GetString("ALBUM"), m_rm.GetString("NEW_ALBUM_NAME"), ref newAlbum) == DialogResult.OK)
            //{
            //    foreach (ImageListViewItem item in imageListView1.SelectedItems)
            //    {
            //        item.SetSubItemText(4, newAlbum);
            //        HormigaObject ho = null;
            //        if (!m_dObjects.TryGetValue(item.FileName, out ho))
            //        {
            //            HormigaObject newho = new HormigaObject(item.FileName, 
            //                item.GetSubItemText(2), 
            //                item.GetSubItemText(1),
            //                newAlbum,
            //                m_default_format,
            //                item.FilePath);
            //            m_dObjects.Add(item.FileName, newho);
            //        }
            //        else
            //        {
            //            ho.m_album = newAlbum;
            //        }
            //    }
            //    SaveHormigaObjects();
            //}
        }
        #endregion

        
    }

    #region Serialization
    [Serializable()]
    public class HormigaObject : ISerializable
    {
        public string m_filename = ""; // full path
        public string m_target = ""; // new directory
        public string m_tag = "";
        public string m_album = ""; // album name
        public string m_format = ""; // %A_%m...
        public string m_selFolderPath = ""; // full path

        public HormigaObject(string filename, string target, string tag, string album, string format, string selFolderPath)
        {
            m_filename = filename;
            m_target = target;
            m_tag = tag;
            m_album = album;
            m_format = format;
            m_selFolderPath = selFolderPath;
        }

        //Deserialization constructor.
        public HormigaObject(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            m_filename = (String)info.GetValue("FileName", typeof(string));
            m_target = (String)info.GetValue("Target", typeof(string));
            m_tag = (String)info.GetValue("Tag", typeof(string));
            m_album = (String)info.GetValue("Album", typeof(string));
            m_format = (String)info.GetValue("Format", typeof(string));
            m_selFolderPath = (String)info.GetValue("SelFolder", typeof(string));
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("FileName", m_filename);
            info.AddValue("Target", m_target);
            info.AddValue("Tag", m_tag);
            info.AddValue("Album", m_album);
            info.AddValue("Format", m_format);
            info.AddValue("SelFolder", m_selFolderPath);
        }
    }
    #endregion
}
