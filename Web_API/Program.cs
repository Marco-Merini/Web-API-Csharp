using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Web_API
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Adicionar handler para exceções não tratadas
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro crítico na aplicação: {ex.Message}\n\nDetalhe: {ex.StackTrace}",
                    "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            MessageBox.Show($"Erro não tratado do AppDomain: {ex.Message}\n\nDetalhe: {ex.StackTrace}",
                "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Erro na thread da UI: {e.Exception.Message}\n\nDetalhe: {e.Exception.StackTrace}",
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}