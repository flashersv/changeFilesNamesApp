﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace changeFilesNamesApp
{
    public partial class Form1 : Form
    {
        private string rutaGeneral = "";
        private DirectoryInfo di;
        private CheckBox folderUnificado;
        private bool corrupcion = false;
        private Dictionary<string, string> aceptacionMasArticulos;
        private string rutaParaMasArticulos = "";
        private Button masArticulos;
        private string letras = "";

        public Form1()
        {
            InitializeComponent();
            
            folderUnificado = checkBox3;
            masArticulos = button2;

            if (string.IsNullOrEmpty(folderDir.Text)) 
            {
                masArticulos.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Reset();
            folder.SelectedPath = rutaGeneral;
            folder.ShowDialog();

            if (folder.SelectedPath.Length > 0)
            {
                rutaGeneral = folder.SelectedPath;
                folderDir.Text = rutaGeneral;
                masArticulos.Enabled = true;
                CargaDeArchivos(rutaGeneral);
                dataGridView1.ClearSelection();
            }
          
        }

        private void extractBtn_Click(object sender, EventArgs e)
        {
            foreach(DataGridViewRow r in dataGridView1.SelectedRows)
            {
                string zipFileName = @rutaGeneral + "/" + r.Cells[0].Value.ToString();
                string zipFilesDestiny = @rutaGeneral + "/" + r.Cells[0].Value.ToString();
                
                zipFilesDestiny = zipFilesDestiny.Substring(0, zipFilesDestiny.Length - 4);
                di = new DirectoryInfo(zipFilesDestiny);

                if (!di.Exists)
                {
                    ZipFile.ExtractToDirectory(zipFileName, zipFilesDestiny);
                }
                else
                {
                    MessageBox.Show("El folder ya existe... deberá eliminarlo");
                }
            }

            CargaDeArchivos(rutaGeneral);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int ctr = 0;
            for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
            {
                if (dataGridView1.SelectedRows[i].Cells[1].Value.ToString().ToLower() == ".zip")
                {
                    ctr++;
                }
            }

            if (ctr > 0)
            {
                extractBtn.Enabled = true;
            }
            else
            {
                extractBtn.Enabled = false;
            }
            /*foreach(DataGridViewRow fz in dataGridView1.SelectedRows)
            {
                if(fz.Cells[1].Value.ToString().ToLower() ==  ".zip")
                {
                    extractBtn.Enabled = true;
                }
                else
                {
                    extractBtn.Enabled = false;
                }
            }*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            dataGridView1.Rows[0].Selected = false;
        }

        /// <summary>
        /// Leer los folders donde se leeran archivos existentes
        /// </summary>
        public void CargaDeArchivos(string ruta)
        {
            di = new DirectoryInfo(ruta);
            int ordenNumerico = 1;
            string[] rutaArray = ruta.Split('\\');
            prefijoTxt.Text = rutaArray[rutaArray.Length - 1] + "_";
            corrupcion = false;

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();

            try { 
                foreach (var d in di.GetDirectories())
                {
                    dataGridView2.Rows.Add(d.Name, "Folder");
                }

                foreach (var f in di.GetFiles())
                {
                    if (f.Extension.ToLower() == ".zip")
                    {
                        dataGridView1.Rows.Add(f.Name, f.Extension);
                    }
                    else
                    {
                        if (!folderUnificado.Checked)
                        {
                            dataGridView2.Rows.Add(f.Name, "File", ordenNumerico);
                            ordenNumerico++;
                        }
                        else
                        {
                            dataGridView2.Rows.Add(f.Name, "File", null);
                        }
                    }
                }

                dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
                dataGridView2.Sort(dataGridView2.Columns[0], ListSortDirection.Ascending);

                if(File.Exists(ruta + "/data/ordn.txt"))
                {
                    dataGridView2.Rows.Clear();
                    using (StreamReader sr = new StreamReader(ruta + "/data/ordn.txt"))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] ss = line.Split(':');
                            dataGridView2.Rows.Add(ss[0], "File", ss[1]);
                        }
                    }
                }

                if(dataGridView2.Rows.Count > 0)
                {
                    folderUnificado.Enabled = true;
                }
                else
                {
                    folderUnificado.Enabled = false;
                }
            }
            catch(DirectoryNotFoundException ex)
            {
                MessageBox.Show("Directorio no encontrado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView2.SelectedRows[0].Cells[1].Value.ToString().ToLower() == "folder")
            {
                rutaGeneral = rutaGeneral + @"\" + dataGridView2.SelectedRows[0].Cells[0].Value.ToString();
                CargaDeArchivos(rutaGeneral);
                folderDir.Text = rutaGeneral;
            }
            
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            rutaGeneral = Directory.GetParent(rutaGeneral).ToString();
            folderDir.Text = rutaGeneral;
            CargaDeArchivos(rutaGeneral);
        }

        private void renombrarBtn_Click(object sender, EventArgs e)
        {
            int pocision = 0;
            string agregarAPrefijo = "";
            ArrayList lineasArchivos = new ArrayList();
            int i = 0;

            foreach(DataGridViewRow row in dataGridView2.Rows) 
            {
                if (row.Cells[1].Value.ToString().ToLower().Equals("file")) ;
                {
                    i++;
                }
            };

            i = 1;

            if (dataGridView2.Rows.Count < 1)
            {
                return;
            }

            foreach(DataGridViewRow d in dataGridView2.Rows)
            {
                if (d.Cells[1].Value.ToString().ToLower() != "folder")
                {
                    if (d.Cells[2].Value == null || d.Cells[2].Value == DBNull.Value || String.IsNullOrWhiteSpace(d.Cells[2].Value.ToString()) || Convert.ToInt32(d.Cells[2].Value) <= 0)
                    {
                        if (!folderUnificado.Checked)
                        {
                            MessageBox.Show("Existen datos inválidos en la columna 'orden'");
                            return;
                        }
                    }

                    foreach (DataGridViewRow x in dataGridView2.Rows)
                    {
                        if (x.Index != pocision)
                        {
                            if (Convert.ToInt32(d.Cells[2].Value) == Convert.ToInt32(x.Cells[2].Value))
                            {
                                if (!folderUnificado.Checked)
                                {
                                    MessageBox.Show("Hay orden(es) repetida(s)");
                                    return;
                                }
                            }
                        }
                    }
                    
                }
                pocision++;
            }

            if(File.Exists(rutaGeneral + "/data/ordn.txt"))
            {
                foreach (DataGridViewRow d in dataGridView2.Rows) 
                {
                    try
                    {
                        File.Move(rutaGeneral + "/" + d.Cells[0].Value.ToString(), rutaGeneral + "/data/" + d.Cells[0].Value.ToString());
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        if (!corrupcion)
                        {
                            MessageBox.Show("Uno o más de los archivos, no se encuentra; pudo haber sido borrado manualmente o el folder está corrompido");
                            corrupcion = true;
                        }
                    }
                }
            }

            foreach(DataGridViewRow d in dataGridView2.Rows)
            {
                if (d.Cells[1].Value.ToString().ToLower() != "folder")
                {
                    if (folderUnificado.Checked)
                    {
                        using (Image file = Image.FromFile(rutaGeneral + "/" + d.Cells[0].Value.ToString()))
                        {
                            if (file.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) || file.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) || file.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif) || file.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                            {
                                //Bitmap img = new Bitmap(rutaGeneral + "/" + d.Cells[0].Value.ToString());

                                if (file.Width == 735 && file.Height == 735)
                                {
                                    agregarAPrefijo = "_G";
                                }
                                else if (file.Width == 535 && file.Height == 535)
                                {
                                    agregarAPrefijo = "_M";
                                }
                                else
                                {
                                    agregarAPrefijo = "_P";
                                }
                            }
                        }

                    }

                    string orden = d.Cells[2].Value == null ? "" : d.Cells[2].Value.ToString();

                    if (Convert.ToInt32(d.Cells[2].Value) == 1)
                    {
                        if (File.Exists(rutaGeneral + "/data/ordn.txt"))
                        {
                            File.Move(rutaGeneral + "/data/" + d.Cells[0].Value.ToString(), rutaGeneral + "/" + prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + ".jpg");
                        }
                        else
                        {
                            File.Move(rutaGeneral + "/" + d.Cells[0].Value.ToString(), rutaGeneral + "/" + prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + ".jpg");
                        }
                        lineasArchivos.Add(prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + ".jpg:1");
                    }
                    else
                    {
                        if (File.Exists(rutaGeneral + "/data/ordn.txt"))
                        {
                            try
                            {
                                File.Move(rutaGeneral + "/data/" + d.Cells[0].Value.ToString(), rutaGeneral + "/" + prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + "_" + orden + ".jpg");
                            }
                            catch (System.IO.FileNotFoundException ex)
                            {
                                if (!corrupcion)
                                {
                                    MessageBox.Show("Uno o más de los archivos, no se encuentra; pudo haber sido borrado manualmente o el folder está corrompido");
                                    corrupcion = true;
                                }
                            }
                        }
                        else
                        {
                            File.Move(rutaGeneral + "/" + d.Cells[0].Value.ToString(), rutaGeneral + "/" + prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + (!checkBox3.Checked ? "_" : "") + orden + ".jpg");
                        }
                        i++;
                        lineasArchivos.Add(prefijoTxt.Text.Substring(0, prefijoTxt.Text.Length - 1) + agregarAPrefijo + (!checkBox3.Checked ? "_" : "") + orden + ".jpg:" + orden);
                    }
                        
                }
            }
            if (!Directory.Exists(rutaGeneral + "/data/"))
            {
                Directory.CreateDirectory(rutaGeneral + "/data/");
            }
            if (File.Exists(rutaGeneral + "/data/ordn.txt"))
            {
                File.Delete(rutaGeneral + "/data/ordn.txt");
            }

            //string[,] losArchivosImg = lineasArchivos.ToArray(typeof(string)) as string[lineasArchivos.Count, 2];

            using (StreamWriter st = File.CreateText(rutaGeneral + "/data/ordn.txt"))
            {
                foreach (var s in lineasArchivos)
                {
                    st.WriteLine(s);
                }
            }

            CargaDeArchivos(rutaGeneral);

            if (checkBox1.Checked)
            {
                CrearArticulos();
            }

            /*DialogResult mensaje =  MessageBox.Show("¿Este producto tiene más artículos además de los básicos?", "Más artículos", MessageBoxButtons.YesNo);
            if (mensaje == DialogResult.Yes) 
            {
                MasArticulos ma = new MasArticulos(rutaGeneral);
                ma.ShowDialog();
                aceptacionMasArticulos = ma.archivos;

                if (aceptacionMasArticulos != null) {
                    CrearMasArticulos(aceptacionMasArticulos);
                }
                else { 
                    return;
                }
            }*/

        }

        private void CrearMasArticulos(Dictionary<string,string> datos, string rutaDondeGuardar)
        {
            foreach (var i in datos) 
            {
                try
                {
                    FileInfo fi = new FileInfo(i.Key);

                    if (fi.Extension == ".jpg" || fi.Extension == ".png" || fi.Extension == ".gif" || fi.Extension == ".bmp")
                    {
                        string nombreArchivo = i.Value.Remove(i.Value.Length - 4);
                        nombreArchivo = nombreArchivo + fi.Extension;
                        nombreArchivo = nombreArchivo.Replace('\\', '/');
                        string[] nombreArchivoArray = nombreArchivo.Split('/');
                        nombreArchivo = nombreArchivoArray[nombreArchivoArray.Length - 1];

                        /*if (!Directory.Exists(rutaGeneral + "/articulos/" + nombreArchivo))
                        {
                            File.Move(i.Key, Directory.GetParent(rutaGeneral) + "/articulos/" + nombreArchivo);
                        }
                        else
                        {
                            File.Move(i.Key, rutaGeneral + "/articulos/" + nombreArchivo);
                        }*/

                        File.Move(i.Key, rutaDondeGuardar + "/" + nombreArchivo);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }

        private void dataGridView2_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView2.SelectedRows[0].Cells[1].Value.ToString().ToLower() != "folder")
            {
                if (!folderUnificado.Checked)
                {
                    dataGridView2.SelectedRows[0].Cells[2].ReadOnly = false;
                }
                else
                {
                    dataGridView2.SelectedRows[0].Cells[2].ReadOnly = true;
                }
            }
        }

        public void CrearArticulos()
        {
            string parentFolder = "";
            ArrayList nombreArticulos = new ArrayList();
            string nombreProducto = "";
            string rutaFolderArticulos = checkBox2.Checked ? rutaGeneral : Directory.GetParent(rutaGeneral).ToString();

            parentFolder = Directory.GetParent(rutaGeneral).ToString();
            di = new DirectoryInfo(rutaGeneral);
            ArrayList archivos = new ArrayList();

            foreach (var d in di.GetFiles())
            {
                archivos.Add(d.Name);
            }

            foreach (var s in di.GetFiles())
            {
                nombreProducto = s.Name;
                Regex regExp = new Regex(@"[a-zA-Z0-9]+_[a-zA-Z0-9]_\d\.[a-zA-Z]+");
                if (!regExp.IsMatch(nombreProducto))
                {
                    nombreArticulos.Add(s.Name);

                }
            }

            di = new DirectoryInfo(rutaFolderArticulos + @"\articulos");
            if (!di.Exists)
            {
                di.Create();
            }

            foreach (string a in nombreArticulos)
            {
                string articuloP1 = a.Insert(4, "-");
                string[] articuloP2 = articuloP1.Split('_');
                string articuloP3 = articuloP2[0].Remove(articuloP2[0].Length - 2);
                string articuloFinal = articuloP3 + "_" + articuloP2[1];

                string rutaOrigen = rutaGeneral + "/" + a;
                string rutaDestino = rutaFolderArticulos + "/articulos/" + articuloFinal;

                rutaOrigen = rutaOrigen.Replace('\\', '/');
                rutaDestino = rutaDestino.Replace('\\', '/');

                File.Copy(rutaOrigen, rutaDestino, true);
            }
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                MessageBox.Show("Primero debe marcar opción para generar artículos");
                checkBox2.Checked = false;
            }
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            int numeroOrden = 1;

            if (folderUnificado.Checked)
            {
                scanAndgroupBtn.Enabled = false;
                //--

                int cuentaFiles = 0;

                foreach (DataGridViewRow r in dataGridView2.Rows)
                {
                    if (r.Cells[1].Value.ToString().ToLower().Equals("file"))
                    {
                        cuentaFiles++;
                    }
                }

                if (cuentaFiles > 3)
                {
                    MessageBox.Show("No pueden ser más de 3 fotos básicas");
                    folderUnificado.Checked = false;
                    scanAndgroupBtn.Enabled = true;
                    return;
                }

                if (cuentaFiles <= 3)
                {
                    di = new DirectoryInfo(rutaGeneral);
                    int _g = 0, _m = 0, _p = 0;
                    foreach (var f in di.GetFiles())
                    {
                        if (f.Extension.Equals(".jpg") || f.Extension.Equals(".png") || f.Extension.Equals(".gif") || f.Extension.Equals(".bmp"))
                        {
                            if(f.Name.IndexOf("_G") > -1 || f.Name.IndexOf("_G.") > -1)
                            {
                                _g++;
                            }
                            else if (f.Name.IndexOf("_M") > -1 || f.Name.IndexOf("_M_") > -1)
                            {
                                _m++;
                            }
                            else if (f.Name.IndexOf("_P") > -1 || f.Name.IndexOf("_P") > -1)
                            {
                                _p++;
                            }
                        }
                    }

                    if(_g < 2  && _m < 2 && _p < 2)
                    {
                        //--
                    }
                    else
                    {
                        MessageBox.Show("Para opción de Fotos básicas de producto, no debe haber más de una foto, con el nombre de nomenclatura permitida");
                        folderUnificado.Checked = false;
                        scanAndgroupBtn.Enabled = true;
                        return;
                    }
                }

            }
            else
            {
                scanAndgroupBtn.Enabled = true;
            }

            foreach (DataGridViewRow c in dataGridView2.Rows)
            {
                if (folderUnificado.Checked)
                {
                    c.Cells[2].Value = null;
                    scanAndgroupBtn.Enabled = false;
                }
                else
                {
                    if (c.Cells[1].Value.ToString().ToLower().Equals("file"))
                    {
                        c.Cells[2].Value = numeroOrden;
                        numeroOrden++;
                    }
                    scanAndgroupBtn.Enabled = true;
                }
            }


        }

        private void scanAndgroupBtn_Click(object sender, EventArgs e)
        {
            string rutaDeDestino, rutaDeLlegada = "";
            string[] prefijoRuta = rutaGeneral.Split('\\');

            foreach (DataGridViewRow f in dataGridView2.Rows)
            {
                if (f.Cells[1].Value.ToString().ToLower().Equals("folder")) { goto FinalLoop; }

                FileInfo file = new FileInfo(rutaGeneral + "/" + f.Cells[0].Value.ToString());

                if (file.Extension == ".jpg" || file.Extension == ".png" || file.Extension == ".gif" || file.Extension == ".bmp")
                {
                    using (Image img = Image.FromFile(rutaGeneral + "/" + file.Name))
                    {
                        if (img.Width == 735 && img.Height == 735)
                        {
                            if (!Directory.Exists(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_G/"))
                            {
                                Directory.CreateDirectory(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_G/");
                            }

                            rutaDeLlegada = rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_G/" + file.Name;
                        }
                        else if (img.Width == 535 && img.Height == 535)
                        {
                            if (!Directory.Exists(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_M/"))
                            {
                                Directory.CreateDirectory(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_M/");
                            }
                            rutaDeLlegada = rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_M/" + file.Name;
                        }
                        else if (img.Width == 135 && img.Height == 135)
                        {
                            if (!Directory.Exists(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_P/"))
                            {
                                Directory.CreateDirectory(rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_P/");
                            }
                            rutaDeLlegada = rutaGeneral + "/" + prefijoRuta[prefijoRuta.Length - 1] + "_P/" + file.Name;
                        }
                    }
                }
                rutaDeDestino = file.FullName;
                File.Move(rutaDeDestino, rutaDeLlegada);

            FinalLoop:
                Console.WriteLine("Ignora folder");
            }
            CargaDeArchivos(rutaGeneral);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MasArticulos ma = new MasArticulos(rutaGeneral);
            ma.ShowDialog();
            aceptacionMasArticulos = ma._Archivos;
            rutaParaMasArticulos = ma._Ruta;

            if (aceptacionMasArticulos != null)
            {
                CrearMasArticulos(aceptacionMasArticulos, rutaParaMasArticulos);
            }
            else
            {
                return;
            }
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Aplicación desarrollada por Ludwin Rodríguez", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void folderDir_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                rutaGeneral = folderDir.Text;

                if (Directory.Exists(rutaGeneral))
                {
                    button2.Enabled = true;
                }

                CargaDeArchivos(rutaGeneral);
            }
        }

        private void buscadorTxt_KeyUp(object sender, KeyEventArgs e)
        {
            letras = buscadorTxt.Text;
            Console.WriteLine(letras);
            int en = 0;

            foreach (DataGridViewRow r in dataGridView2.Rows)
            {
                if (r.Cells[0].Value.ToString().ToLower().Substring(0).Equals(letras.ToLower()))
                {
                    dataGridView2.FirstDisplayedScrollingRowIndex = r.Index;
                    dataGridView2.Rows[r.Index].Selected = true;
                }
            }
        }
    }
}       

