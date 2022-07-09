using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace changeFilesNamesApp.classes
{
    class Browser
    {
        private string unaRuta = "";
        public Browser(string _ruta) 
        {
            unaRuta = _ruta;
        }
        public void DialogoBrowser() 
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.SelectedPath = unaRuta;
            f.ShowDialog();
        }
    }
}
