using PDF_Page_Counter.Cripto;
using System;
using System.IO;
using System.Windows.Forms;

namespace PDF_Page_Counter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //simple licence control
            DateTime dataRun;
            var file = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "sp.bin");
            if (!File.Exists(file))
            {
                dataRun = DateTime.Now.AddDays(14);
                var dataTiks = dataRun.Ticks.ToString();
                var dataCripto = CAppSettings.EncryptString(dataTiks);
                File.WriteAllText(file, dataCripto);
                //  MessageBox.Show("Licença não encontrada.", "Licence Validator", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Run(new FrmMain(dataRun.ToShortDateString()));
                return;
            }
            try
            {
                var hoje = DateTime.Now;
                var cData = File.ReadAllText(file);
                var tData = CAppSettings.DecryptString(cData);
                dataRun = DateTime.FromBinary(long.Parse(tData));

                if (hoje >= dataRun)
                {
                    MessageBox.Show("Esta aplicação perdeu o prazo de validade.", "Licence Validator", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            catch (Exception)
            {

                throw;
            }
            Application.Run(new FrmMain(dataRun.ToShortDateString()));
            // Application.Run(new FrmMain());
        }
    }
}
