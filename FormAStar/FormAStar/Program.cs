using System;
using System.Windows.Forms;

namespace FormAStar // Asegúrate que esto coincida con el paso 1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Aquí arranca tu ventana
        }
    }
}