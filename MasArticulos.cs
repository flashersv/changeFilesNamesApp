using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace changeFilesNamesApp
{
    public partial class MasArticulos : Form
    {
        DirectoryInfo di;
        string ruta = "";
        public Dictionary<string, string> archivos = new Dictionary<string, string>();
        bool ifAceptarClicked = false;

        public MasArticulos(string ruta)
        {
            InitializeComponent();
            this.ruta = ruta;
        }

        private void MasArticulos_Load(object sender, EventArgs e)
        {
            CargarDirectoryInfo(ruta);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Cells[2].Value.ToString().ToLower().Equals("folder")) 
            {
                CargarDirectoryInfo(this.ruta + "/" + dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
            }
            else if (dataGridView1.SelectedRows[0].Cells[2].Value.ToString().ToLower().Equals("file"))
            {
                dataGridView1.SelectedRows[0].Cells[1].ReadOnly = false;
            }
        }

        private void CargarDirectoryInfo(string ruta)
        {
            this.ruta = ruta;

            try
            {
                di = new DirectoryInfo(this.ruta);
            }
            catch(Exception ex)
            {
                MessageBox.Show("No se ha seleccionado folder");
                this.Close();
                return;
            }
            dataGridView1.Rows.Clear();

            foreach (var d in di.GetDirectories())
            {
                dataGridView1.Rows.Add(d.Name, null, "Folder");
            }

            foreach (var d in di.GetFiles())
            {
                dataGridView1.Rows.Add(d.Name, null, "File");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            archivos = null;
            this.Close();
        }

        private void aceptarBtn_Click(object sender, EventArgs e)
        {
            foreach(DataGridViewRow r in dataGridView1.Rows)
            {
                if(r.Cells[1].Value != null && !String.IsNullOrEmpty(r.Cells[1].Value.ToString()))
                {
                    archivos.Add(ruta + "/" + r.Cells[0].Value.ToString(), ruta + "/" + r.Cells[1].Value.ToString());
                }
            }
            ifAceptarClicked = true;
            this.Close();
        }

        private void MasArticulos_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ifAceptarClicked)
            {
                archivos = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CargarDirectoryInfo(Directory.GetParent(ruta).ToString());
        }
    }
}
