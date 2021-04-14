using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using static System.Int32;

namespace PDF_Page_Counter
{
    public partial class MainForm : Form
    {
        private static readonly string[] SizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
        private readonly BackgroundWorker _bgw;
        private List<PdfToCount> _pdfs;
        public string[] RootFiles { get; private set; }

        public MainForm(string validadeInfo)
        {
            _pdfs = new List<PdfToCount>();
            Application.VisualStyleState = VisualStyleState.NonClientAreaEnabled;
            InitializeComponent();
            _bgw = new BackgroundWorker();
            _bgw.DoWork += bgw_DoWork;
            _bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
            this.Text += " - Finaliza em " + validadeInfo;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                this.RootFiles = s;
                _bgw.RunWorkerAsync(s);
            }
            while (_bgw.IsBusy)
            {
                Form overlay = new WorkingOverlay();
                overlay.StartPosition = FormStartPosition.CenterParent;
                overlay.Size = Size;
                //overlay.ShowDialog(this);
                Application.DoEvents();
            }
        }

        private void AddFileToListview(string fullFilePath)
        {
            PdfToCount pdf = new PdfToCount();
            Cursor.Current = Cursors.WaitCursor;
            if (!File.Exists(fullFilePath))
                return;
            var fileName = Path.GetFileName(fullFilePath);
            var dirName = Path.GetDirectoryName(fullFilePath);
            if (dirName != null && dirName.EndsWith(Convert.ToString(Path.DirectorySeparatorChar)))
                dirName = dirName.Substring(0, dirName.Length - 1);
            var itm = listView1.Items.Add(fileName);
            pdf.Arquivo = fileName;
            if (fileName != null)
            {
                // ReSharper disable once UnusedVariable
                var fileInfo = new FileInfo(fileName);
            }
            var length = new FileInfo(fullFilePath).Length;
            pdf.Tamanho = length;
            //size column
            itm.SubItems.Add(SizeSuffix(length));

            //catch file problems
            try
            {
                var pdfReader = new PdfReader(fullFilePath);
                var numberOfPages = pdfReader.NumberOfPages;
                itm.SubItems.Add("Bom");                
                itm.SubItems.Add(numberOfPages.ToString());                         
                itm.SubItems.Add(dirName);

                pdf.Status = "Bom";
                pdf.Paginas = numberOfPages;
                pdf.Caminho = dirName;
                pdf.Erro = "";
            }
            catch (Exception e)
            {
                itm.SubItems.Add("Arquivo corrompido");
                itm.SubItems.Add("0");
                itm.SubItems.Add(dirName);
                itm.SubItems.Add(e.Message);

                pdf.Status = "Arquivo corrompido";
                pdf.Paginas = 0;
                pdf.Caminho = dirName;
                pdf.Erro = e.Message;
            }

            //calculate items count with linq
            var countItems = listView1.Items.Cast<ListViewItem>().Count();
            toolStripStatusLabel3.Text = countItems.ToString();

            //calculate total pages count with linq
            var countTotalPages = listView1.Items.Cast<ListViewItem>().Sum(item => Parse(item.SubItems[3].Text));
            toolStripStatusLabel4.Text = countTotalPages.ToString();
            _pdfs.Add(pdf);
            Cursor.Current = Cursors.Default;
        }

        private static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value < 0) return "-" + SizeSuffix(-value);
            if (value == 0) return "0.0 bytes";

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            var mag = (int) Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            var adjustedSize = (decimal) value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            // ReSharper disable once FormatStringProblem
            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

  

        // Clears listview items sets counters to zero
        private void button2_Click(object sender, EventArgs e)
        {
            this.RootFiles = null;
            CleanAll();
        }
        private void CleanAll()
        {
            _pdfs = new List<PdfToCount>();
           
            listView1.Items.Clear();
            toolStripStatusLabel3.Text = @"0";
            toolStripStatusLabel4.Text = @"0";
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Invoke(new Action<object>(args =>
            {
                var handles = (string[]) e.Argument;
                foreach (var s in handles)
                    if (File.Exists(s))
                    {
                        if (string.Compare(Path.GetExtension(s), ".pdf", StringComparison.OrdinalIgnoreCase) == 0)
                            AddFileToListview(s);
                    }
                    else if (Directory.Exists(s))
                    {
                        var di = new DirectoryInfo(s);
                        var files = di.GetFiles("*.pdf",
                            checkBox1.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
                            AddFileToListview(file.FullName);
                    }
            }), e.Argument);
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
          //  ActiveForm?.Hide();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var listView = sender as ListView;
            if (e.Button == MouseButtons.Right)
            {
                var item = listView?.GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    item.Selected = true;
                    contextMenuStrip1.Show(listView, e.Location);
                }
            }
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(listView1.SelectedItems[0].SubItems[4].Text);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = (Parse(toolStripStatusLabel3.Text) - 1).ToString();
            toolStripStatusLabel4.Text = (Parse(toolStripStatusLabel4.Text) - Parse(listView1.SelectedItems[0].SubItems[3].Text)).ToString();
            listView1.SelectedItems[0].Remove();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Planilha Excel|*.xlsx";
                saveFileDialog1.Title = "Exportar";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    string excelFile = saveFileDialog1.FileName;

                    if (_pdfs.Count==0)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show("Não foi possível exportar esta lista");
                        return;
                    }

                    
                    ExportXLSX(excelFile, _pdfs);

                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(excelFile);
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Falha interna, tente novamente " + ex);
                return;
            }


        }

        private void ExportXLSX(string excelFile, List<PdfToCount> pdfs)
        {
            ExcelPackage pck = new ExcelPackage();

            var wsDt = pck.Workbook.Worksheets.Add("Dados");
            wsDt.Cells["A1"].LoadFromCollection(pdfs, true, TableStyles.Medium9);
            wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
            var fi = new FileInfo(excelFile);
            if (fi.Exists)
                fi.Delete();
            pck.SaveAs(fi);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CleanAll();
            if (this.RootFiles!=null)
            {
                  if (this.RootFiles.Length>0)
                    _bgw.RunWorkerAsync(this.RootFiles);
            }
            while (_bgw.IsBusy)
            {
                Form overlay = new WorkingOverlay();
                overlay.StartPosition = FormStartPosition.CenterParent;
                overlay.Size = Size;
                //overlay.ShowDialog(this);
                Application.DoEvents();
            }
        }
    }
}