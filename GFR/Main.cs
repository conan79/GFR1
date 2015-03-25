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
using PowerShapeDotNet;
using PowerSolutionDOTNetOLE;
using Ini;


namespace GFR
{
    public partial class Main : Form
    {
        #region VARIABLES
        //variables de entorno
        string _pathDirectory = "";                         //la ruta del directory seleccionado
        string[] files;                                     //ficheros en el directory y subdirectorios
        List<string> _datarow1;                             //datos de la primero columna de la tabla
        List<string> _datarow2;                             //datos de la segunda columna de la tabla
        string _param1;                                     //parametro primero 
        string _param2;                                     //parametro segundo
        List<string> _lines = new List<string>();           //lineas de comandos
        List<String> _headLines = new List<string>();       //lineas de la cabezera
        List<BaseHeadParams> headParams = new List<BaseHeadParams>();   //parametro de la cabezera
        int headParamsCount = 0;                            //numero de parametros de cabezera
        string _pathmultifiles = "";                        //ruta de los ficheros seleecionados en elselector de multiple ficheros de nombre
        List<string> xmltallas = new List<string>();        //listado de las tallas del xml
        //variables para powermill
        clsPowerMILLInstance PowerMill;
        bool PowerMillisConnect = false;
        List<PowerMillData> PMData = new List<PowerMillData>();//Datos del arbol de powermill
        bool OnlyOneCheckItem = false;
        bool isGenericName = false;
        //fichero de inicio
        IniFile myIni;

        #endregion
        Panel xPanel = new Panel();

        public Main()
        {
            InitializeComponent();
            this.Size = new Size(325, 700);
           // this.Size = new Size(1350, 700);
        //    PMillDotNet pm = new PMillDotNet();
          //  pm.Crear_Conexion();
          // pm.Crear_Nueva_Conexion();
            //ConnectToPowrMill();
            CheckIniFile();
            string aaaa = "dasdjkl_sajd_ksa_34";
            aaaa =  aaaa.GetGenericName();
          //  MessageBox.Show(aaaa);
        }
        /// <summary>
        /// Selecciona el directorio principal 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectoySetBTN_MouseClick(object sender, MouseEventArgs e)
        {
            (sender as Button).BackColor = Color.MediumTurquoise;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // The user selected a folder and pressed the OK button.
                // We print the number of files found.
                files = null;
                CrearAbrolDeFicheros(folderBrowserDialog1.SelectedPath);
            }
        }

        private void CrearAbrolDeFicheros(string _pathD)
        {
            files = Directory.GetFiles(_pathD, "*.txt", SearchOption.AllDirectories);

            if (string.IsNullOrEmpty(DirectoyNameTXT.Text))
            {
                DirectoyNameTXT.Text = folderBrowserDialog1.SelectedPath;
            }

            myIni.IniWriteValue("DirectoryGFR", "MainDirectory", DirectoyNameTXT.Text);
            // MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
            //vaciamos el tree view
            DirectoryView.Nodes.Clear();
            foreach (string node in files)
            {
                DirectoryView.Nodes.Add(Path.GetFileName(node));
            }
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
            saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.ShowDialog();
            string _filename = saveFileDialog1.FileName;
            if(FP_RTB.Text != "")
            {
                File.WriteAllText(_filename, FP_RTB.Text);
            }
           
        }

        private void CrearParametroBTN_Click(object sender, EventArgs e)
        {
            string _param = "";
            
            if(!string.IsNullOrEmpty( CrearParamentroTXT.Text))
            {
                _param = "["+CrearParamentroTXT.Text+"]";
                _param1 = _param;
                if (FP_RTB.Text.Contains(_param))
                {
                    MessageBox.Show(string.Format("Parametro encontrado."));
                    ParametrosDGV.Columns[0].HeaderText = _param;
                    this.Size = new Size(1005, 700);

                }
                else
                {
                    MessageBox.Show(string.Format("Parametro no encontrado."));
                }
            }
        }

        private void GenerarFicheroComandosBTN_Click(object sender, EventArgs e)
        {
            GenerarFicheroComandosBTN.BackColor = Color.MediumTurquoise;
            //desglosamos y almacenamos los tipos de lineas(cabezera y comandos)
            ColletHeadandCommandParams();

            if (CollectDataRows())
            {
                if(!CreateHeadParamsPanel())
                {
                    GenerateCommandFile();
                }  
            }
            else
            {
                MessageBox.Show("Rellena los datos");
            }

           
        }
        /// <summary>
        /// Genera un panel modal con los parametros de la cabezera para introducirlos
        /// </summary>
        private bool CreateHeadParamsPanel()
        {
            if (headParams.Count > 0)
            {
                Utils.CreateLabelEdit(HeadparamsPNL, headParams);
                xPanel.Left = 0;
                xPanel.Top = 0;
                xPanel.Size = new Size(this.Size.Width, this.Size.Height);
                this.Controls.Add(xPanel);
                xPanel.BringToFront();
                HeadparamsPNL.Left = 350;
                HeadparamsPNL.Size = new Size(330,(headParams.Count * 30));
                HeadparamsPNL.BringToFront();
                headParams.GetHeadParamsFromFile();
                this.Refresh();
                return true;
            }
            return false;
        }
        /// <summary>
        /// recojemos los datos que hemos introducido en la tabla de parametros
        /// usando dos lista de strings
        /// </summary>
        private bool CollectDataRows()
        {
            _datarow1 = new List<string>();
            _datarow2 = new List<string>();
            if (!string.IsNullOrEmpty(ParametrosDGV.Columns[0].HeaderText) || ParametrosDGV.Columns[0].HeaderText == "C1")
            {
                if (ParametrosDGV.RowCount > 0)
                {
                    _datarow1 = new List<string>();
                    for (int cnt = 0; cnt < ParametrosDGV.RowCount ; cnt++)
                    {
                        if (ParametrosDGV.Rows[cnt].Cells[0].Value != null)
                        {
                            string _datarow = ParametrosDGV.Rows[cnt].Cells[0].Value.ToString();
                            _datarow1.Add(_datarow);
                            //  MessageBox.Show(string.Format(_datarow));
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(ParametrosDGV.Columns[1].HeaderText) || ParametrosDGV.Columns[1].HeaderText == "C2")
            {
                if (ParametrosDGV.RowCount > 0)
                {
                    _datarow2 = new List<string>();
                    for (int cnt = 0; cnt < ParametrosDGV.RowCount ; cnt++)
                    {
                        if(ParametrosDGV.Rows[cnt].Cells[1].Value != null)
                        { 
                            string _datarow = ParametrosDGV.Rows[cnt].Cells[1].Value.ToString();
                            _datarow2.Add(_datarow);
                            //  MessageBox.Show(string.Format(_datarow));
                        }
                    }
                }
                if (_datarow1.Count != _datarow2.Count)
                {
                    MessageBox.Show("Faltan datos en las columnas de parametros.");
                    return false;
                }
            }
            if (_datarow1.Count < 1)
            {
                MessageBox.Show("Faltan datos en las columnas de parametros.");
                return false;
            }
      
            return true;
        }
        /// <summary>
        /// recojemos las lineas de cabecera y las de comandos
        /// </summary>
        private void ColletHeadandCommandParams()
        {
            _headLines.Clear();
            _lines.Clear();
            //leemos todas las lineas y las separamos en dos listas, una para la cabezera y otra para los comandos      
            foreach (string l in FP_RTB.Lines)
            {
                //diferenciamos si son lines de cabecera o no
                if (l.Contains("##"))
                {
                    _headLines.Add(l);
                }
                else
                {
                    _lines.Add(l);
                }
            }
            //ahora tenemos que cojer los parametros de la cabezara desglosarlos y almacenarlos
            headParamsCount = 0;
            headParams.Clear();
            foreach (string l in _headLines)
            {
                headParams.Add(new BaseHeadParams(l));
                headParamsCount++;
                if (string.IsNullOrEmpty(headParams[headParams.Count - 1].varParam) || string.IsNullOrWhiteSpace(headParams[headParams.Count - 1].varParam))
                {
                    headParams.RemoveAt(headParams.Count - 1);
                    headParamsCount--;
                }
            }
        }
        /// <summary>
        /// Generamos el texto para el fichero con todos los comandos
        /// </summary>
        private void GenerateCommandFile()
        {
            FicheroComandosTXT.Text = "";
         
            //comprobamos si tenemos parametro de cabezera y mostramos una ventana con los mismo e introducir los datos
            if (headParams.Count > 0)
            {
                
                //rellenamos los valores añadidos en los text box del panel de introduccion de parametros de cabezera
                foreach (BaseHeadParams hp in headParams)
                {
                    foreach (CustomControls cc in Utils.myCustomControls)
                    {
                        if (String.Equals (  cc.ParamName , hp.varParam))
                        {
                            hp.varValue = cc.TextBoxCC.Text;
                        }
                    }
                }
               
                //como tenemos datos rellenamos el fichero con la cabezera primero
               // foreach (BaseHeadParams hp in headParams)
                foreach (string hdLine in _headLines)
                {
                    string headtxtline = "";
                   // foreach (string hdLine in _headLines)
                    foreach (BaseHeadParams hp in headParams)
                    {
                        //si tiene el tipo << >> es un parametro3 base de la cabezera
                        if (hdLine.Contains(BaseClasses._headformat3) && hdLine.Contains(hp.varParam))
                        {
                            headtxtline = hdLine.Replace(BaseClasses._headformat3, hp.varValue + ';');
                            FicheroComandosTXT.Text += headtxtline + "\n";
                        }
                        else //si tiene el tipo < > es un parametro2 base de la cabezera
                        if (hdLine.Contains(BaseClasses._headformat2) && hdLine.Contains(hp.varParam))
                        {
                            headtxtline = hdLine.Replace(BaseClasses._headformat2, hp.varValue+';');
                            FicheroComandosTXT.Text += headtxtline + "\n";
                        }
                    }
                }
            }

            int cnt = 0;
            foreach (string data in _datarow1)
            {
                foreach(string thisline in _lines)
                {
                    string auxline = thisline;
                    auxline = auxline.Replace(_param1, data);
                    //comprobamos si tenemos datos en la columna 2
                    if (_datarow2.Count > 0)
                    {
                        auxline = auxline.Replace(_param2, _datarow2[cnt]);
                    }
                    foreach (BaseHeadParams hp in headParams)
                    {
                        if(auxline.Contains(hp.varParam))
                        {
                            auxline = auxline.Replace(hp.varParam, hp.varValue);
                        }
                    }
                   
                    FicheroComandosTXT.Text += auxline + "\n";
                }
                cnt++;
            }
            tabControl1.SelectedTab = tabPage2;
        }
        /// <summary>
        /// comprobamos y recojemos los parametros [AA],[BB] en el fichero
        /// </summary>
        private Boolean CheckParamsInFile()
        {
            bool haveParams = false;
            _param1 = "";
            _param2 = "";
            //comprobamos que no este vacio
            if (FP_RTB.Lines.Count() > 0)
            {
                for (int cnt = 0; cnt < FP_RTB.Lines.Count() - 1; cnt++)
                {
                    string _line = FP_RTB.Lines[cnt];
                    string _aux = "";
                    bool isColletionParam = false;

                    foreach (char c in _line)
                    {
                        if (c == '[')
                        {
                            isColletionParam = true;
                        }

                        if (isColletionParam)
                        {
                            _aux += c;
                        }

                        if (c == ']')
                        {
                            isColletionParam = false;
                            if (string.IsNullOrEmpty(_param1))
                            {
                                _param1 = _aux;
                                ParametrosDGV.Columns[0].HeaderText = _param1;
                                _aux = "";
                            }
                            if (string.IsNullOrEmpty(_param2))
                            {
                                _param2 = _aux;
                                //comprobamos que no sea una repeticion del mismo parametro
                                if (_param2 == _param1)
                                {
                                    _param2 = "";
                                }
                                else
                                {
                                    ParametrosDGV.Columns[1].HeaderText = _param2;
                                }

                                _aux = "";
                            }
                        }
                   
                    }
                }
                if (!string.IsNullOrEmpty(_param1))
                {
                    haveParams =  true;
                }
            }
            return haveParams;
        }
        /// <summary>
        /// genera una sucesion de numero entre un rango a una columna de datos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarBTN_Click(object sender, EventArgs e)
        {
            CleanBTN_Click(sender, e);
            int selected = ParamsCheck.SelectedIndex;
            int indexColl = 0;
            if (selected != -1)
            {
                indexColl = selected;
                if (string.IsNullOrEmpty(ParametrosDGV.Columns[1].HeaderText) && ParametrosDGV.Columns[1].HeaderText == "C2")
                {
                    indexColl--;
                    ParamsCheck.SelectedIndex = indexColl;
                }
            }
            else
            {
                MessageBox.Show("Selecciona un parametro el cual quieras modificar."); return;
            }
            
            int min = 0;
            int max = 0;
            if (!string.IsNullOrEmpty(RangeMinTXT.Text) && !string.IsNullOrWhiteSpace(RangeMinTXT.Text))
            {
                if (!string.IsNullOrEmpty(RangeMaxTXT.Text) && !string.IsNullOrWhiteSpace(RangeMaxTXT.Text))
                {
                     min = int.Parse(RangeMinTXT.Text);
                     max = int.Parse(RangeMaxTXT.Text);

                     for (int cnt = 0; cnt <= (max - min); cnt++)
                     {
                         ParametrosDGV.Rows.Add();
                         ParametrosDGV.Rows[cnt].Cells[indexColl].Value = min + cnt;
                     }
                }
                else
                {
                    MessageBox.Show("Introduzca un mumero para el rango Maximo");
                }
            }
            else
            {
                MessageBox.Show("Introduzca un mumero para el rango minimo");
            }
         

        }
        /// <summary>
        /// limpiamos la tabla de parametros de la derecha
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanBTN_Click(object sender, EventArgs e)
        {
            ParametrosDGV.Rows.Clear();
        }
        /// <summary>
        /// abrimos un dialog para seleccionar multiples fichero y recojer sus nombre 
        /// para luego guardar los comandos generados en cada uno de ellos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertaNombreFicherosBTN_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
            List<string> _mfiles = new List<string>();
            NombresFicheroTXT.Text = "";
            foreach (string _files in openFileDialog1.FileNames)
            {
                NombresFicheroTXT.Text +=  Path.GetFileName(_files)+";";
                _mfiles.Add(Path.GetFileNameWithoutExtension(_files));
                _pathmultifiles = Path.GetDirectoryName(_files) + "\\";
                //_mfiles.Add(Path.GetFileName(_files) );
            }
            InsertDataInColumParam(_mfiles);

        }

        private void InsertHeadParamsBTN_Click(object sender, EventArgs e)
        {
            //comprobamos que tengamos datos en todos los textboxes de los parametros
            if (Utils.myCustomControls.CheckValuesOK())
            {
                GenerateCommandFile();
                xPanel.SendToBack();
                HeadparamsPNL.Left = 1500;
                HeadparamsPNL.SendToBack();
                headParams.SaveHeadParamsInFile();
            }
            else
            {
                MessageBox.Show("Faltan datos de parametro de cabezera por introducir.");
            }
        }

        private void guardarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (FicheroComandosTXT.Text != "")
            {
                saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "bch files (*.bch)|*.bch |com files (*.com)|*.com |mac files (*.mac)|*.mac";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.ShowDialog();
                string _filename = saveFileDialog1.FileName;
                if (!string.IsNullOrEmpty(_filename))
                {
                    File.WriteAllText(_filename, FicheroComandosTXT.Text);
                }
             
            }
            else
            {
                MessageBox.Show("No ha generado el fichero de comandos.\n No tiene datos para guardar");
                GenerarFicheroComandosBTN.BackColor = Color.Red;
            }
        }
     
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.ShowDialog();
            string _filename = saveFileDialog1.FileName;
            if(!string.IsNullOrEmpty(_filename))
            {
                File.WriteAllText(_filename, "");
                foreach (int h in Utils.AdditiveInt(410, 20))
                {
                    this.Size = new Size(325 + h, 700);
                    this.Refresh();
                }
            }
        }

        private void DirectoryView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;//DirectoryView.SelectedNode;

            if (node != null)
            {
                _pathDirectory = Utils.GetRealPath(files, node.Text);
                string imageFile = Path.Combine(_pathDirectory, node.Text);
                imageFile = imageFile.Replace("txt", "jpg");
               // MessageBox.Show(string.Format(imageFile));
                if (File.Exists(imageFile))
                {
                    ImageFile_PB.Load(imageFile);
                    ImageFile_PB.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                else
                {
                    ImageFile_PB.Image = null;
                }
            }
        }
        /// <summary>
        /// creamos la conexion a powermill
        /// </summary>
        private void ConnectToPowerMill()
        {
            try
            {
                PowerMill = new clsPowerMILLInstance(clsPowerMILLInstance.StartupOptions.ConnectToExistingOrRunNew, true);
                if (PowerMill.Connect())
                {
                    MessageBox.Show("Conexion realizada a powermill");
                    PowerMillisConnect = true;
                }
            }catch(Exception ex)
            {
                ///
            }

        }
        /// <summary>
        /// Ejecutamos macros individuales a la instancia de PowerMill 
        /// </summary>
        /// <param name="_cmdlines">Lista de commandos</param>
        private void ExcuteCommandsToPowerMill(List<string> _cmdlines)
        {
            if (!PowerMillisConnect) ConnectToPowerMill();

            PowerMill.Execute(FicheroComandosTXT.Lines);
            //foreach (string cmdline in test111(_cmdlines))
            //{
            //    //
            //}
            //foreach (string cmdline in _cmdlines)
            //{
            //    string pmResult = PowerMill.ExecuteEx(cmdline);

            //}
           // PowerMill.CloseOLEConnections();
        }
        IEnumerable<string> test111(List<string> _cmdlines)
        {
            foreach (string cmdline in _cmdlines)
            {
                string pmResult = PowerMill.ExecuteEx(cmdline);
                yield return null;
                yield return pmResult;
            }
        }
        /// <summary>
        /// Comprobamos el fichero de parametros iniciales
        /// realizamos acciones dependiendo de que parametro tengammos
        /// </summary>
        private void CheckIniFile()
        {
            string _pathDini;
            if (File.Exists(".\\GFR.ini"))
            {
                myIni = new IniFile(".\\GFR.ini");
                _pathDini = myIni.IniReadValue("DirectoryGFR", "MainDirectory");
                //MessageBox.Show("Directory inicial: " +_pathDini);
                if (!string.IsNullOrEmpty(_pathDini))
                {
                    DirectoyNameTXT.Text = _pathDini;
                    CrearAbrolDeFicheros(_pathDini);
                }
            }
        }
        /// <summary>
        /// leemos todos los apartados del powermill
        /// </summary>
        private void ReadPowerMillProyect()
        {
            if (PowerMillisConnect)
            {
                ////TRAYECTORIAS - TOOLPATH
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmToolpath, "TRAYECTORIAS"));
                ////HERRAMIENTAS - TOOL
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmTool, "HERRAMIENTAS"));
                ////LIMITES - pmBoundary ((NOTA TAMBIEN INCLUYE NIVELES Y CONJUNTOS))
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmBoundary, "LIMITES"));
                ////PATRONES - PATTERN
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmPattern, "PATRONES"));
                ////PLANOS DE TRABAJO
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmWorkplane, "PLANOS DE TRABAJO"));
                ///
                PMData.Add(new PowerMillData(clsPowerMILLOLE.enumPowerMILLEntityType.pmNCProgram, "PROGRAMASCN"));

                foreach (PowerMillData _pmd in PMData)
                {
                    PowerMill.GetEntityList(_pmd.nameType,ref _pmd.count,ref _pmd.Data,ref _pmd.ID);
                }
            }
        }
        /// <summary>
        /// creamos todos los apartados del power mill
        /// </summary>
        private void CreatePowerMillTreeView()
        {
            int pmtvCount = 0;
            PowerMillTreeView.Nodes.Clear();
            PowerMillTreeView.CheckBoxes = true;
            PowerMillTreeView.BeginUpdate();

            foreach (PowerMillData _pmd in PMData)
            {
                if (_pmd.count > 0)
                {
                    PowerMillTreeView.Nodes.Add(_pmd.name);
                    for (int cnt = 0; cnt < _pmd.count; cnt++)
                    {
                        PowerMillTreeView.Nodes[pmtvCount].Nodes.Add(_pmd.Data[cnt]);
                    }
                    pmtvCount++;
                }
            }

            PowerMillTreeView.EndUpdate();
        }
        /// <summary>
        /// Creamos un arbol solo con el apartado que le indiquemos del powermill
        /// </summary>
        /// <param name="_type"></param>
        private void CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType _type)
        {
            PowerMillTreeView.Nodes.Clear();
            PowerMillTreeView.CheckBoxes = true;
            PowerMillTreeView.BeginUpdate();
            foreach (PowerMillData _pmd in PMData)
            {
                if (_pmd.nameType == _type)
                {
                    PowerMillTreeView.Nodes.Add(_pmd.name);
                    for (int cnt = 0; cnt < _pmd.count; cnt++)
                    {
                        PowerMillTreeView.Nodes[0].Nodes.Add(_pmd.Data[cnt]);
                    }
                    break;
                }
            }
            PowerMillTreeView.EndUpdate();
        }
        /// <summary>
        /// recargamos los datos del arbol de powermill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadBTN_Click(object sender, EventArgs e)
        {
            ReadPowerMillProyect();
        }

       
        /// <summary>
        /// comprobamos en un treeview que items estan seleecionados y los almacenamos
        /// </summary>
        /// <param name="_tw"></param>
        /// <returns></returns>
        private List<string> GetCheckedItemsInTree(TreeView _tw)
        {
            List<string> itemsChecked = new List<string>();
            foreach (TreeNode n in _tw.Nodes)
            {
                foreach (TreeNode n1 in n.Nodes)
                {
                    if (n1.Checked)
                    {
                        if (isGenericName)
                        {
                            itemsChecked.Add(n1.Text.GetGenericName());
                        }
                        else
                        {
                            itemsChecked.Add(n1.Text);
                        }
                    }
                }
            }
            isGenericName = false;
            return itemsChecked;
        }
        /// <summary>
        /// recojemos los datos seleccionas del arbol de powermill y los aplicamos a la columna seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarPM_BTN_Click(object sender, EventArgs e)
        {
            List<string> auxvar = new List<string>();
            auxvar = GetCheckedItemsInTree(PowerMillTreeView);
            InsertDataInColumParam(auxvar);
            ClosePowerMillPanelBTN_Click(sender, e);
            OnlyOneCheckItem = false;
        }
        /// <summary>
        /// rellenamos la columna correspondiente al parametro seleccionado con la lista de parametros seleccionados
        /// </summary>
        /// <param name="_datalist">lista de datos</param>
        private void InsertDataInColumParam(List<string> _datalist)
        {
            for (int i = 0; i < ParamsCheck.Items.Count; i++)
            {
                if (ParamsCheck.GetItemChecked(i))
                {
                    int cnt =( ParametrosDGV.Rows.Count-1);

                    //while (ParametrosDGV.RowCount < _datalist.Count)
                    //{
                    //    ParametrosDGV.Rows.Add();
                    //}
                    for (int x = 0; x < _datalist.Count;x++ )
                    {
                        ParametrosDGV.Rows.Add();
                    }
                    //numero de valores de parametros que vamos a introducir
                    foreach (string s in _datalist)
                    {
                        bool _canSetRowValue = true;
                        //comprobar que el valor no este ya introducido
                        foreach (DataGridViewRow _row in ParametrosDGV.Rows)
                        {
                            if (_row.Cells[i].Value != null)
                            {
                                if (_row.Cells[i].Value.ToString() == s)
                                {
                                    _canSetRowValue = false;
                                    //ParametrosDGV.Rows.RemoveAt(ParametrosDGV.Rows.Count - 1);
                                    break;
                                }
                            }
                        }
                        if (_canSetRowValue)
                        {
                            ParametrosDGV.Rows[cnt].Cells[i].Value = s;
                            if (ParametrosDGV.Columns[1].HeaderText.Contains(BaseClasses._typeparam4))
                            {
                                ParametrosDGV.Rows[cnt].Cells[1].Value = _pathmultifiles;
                            }
                            cnt++;
                        }
                    }
                    //vaciamos filas vacias
                    for (int y = ParametrosDGV.Rows.Count - 1; y >= 0; y-- )
                    {
                        try
                        {
                           // if (string.IsNullOrEmpty(ParametrosDGV.Rows[y].Cells[0].Value.ToString()) && string.IsNullOrEmpty(ParametrosDGV.Rows[y].Cells[1].Value.ToString()))
                            if (ParametrosDGV.Rows[y].Cells[0].Value == null && ParametrosDGV.Rows[y].Cells[1].Value == null)
                            {
                                ParametrosDGV.Rows.RemoveAt(y);
                            }
                        }
                        catch (Exception ex) {// MessageBox.Show(ex.ToString());
                        }
                    }
                }
            }
        }
        //esto es para que solo este un parametro checkeado al mismo tiempo 
        private void ParamsCheck_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked && ParamsCheck.CheckedItems.Count > 0)
            {
                ParamsCheck.ItemCheck -= ParamsCheck_ItemCheck;
                ParamsCheck.SetItemChecked(ParamsCheck.CheckedIndices[0], false);
                ParamsCheck.ItemCheck += ParamsCheck_ItemCheck;
            }
            //comprobar si el parametro tiene parametrizada su inserccion de datos y monstramos solo lo que puede hacer

            PowerMillBTN.Visible = true;
            XmlCustomFile_BTN.Visible = true;
            SecuenciaNumerosPanel.Visible = true;
            NombreFicherosPanel.Visible = true;

            if (ParametrosDGV.Columns[e.Index].HeaderText.Contains(BaseClasses._typeparam1))
            {
                XmlCustomFile_BTN.Visible = false;
                SecuenciaNumerosPanel.Visible = false;
                NombreFicherosPanel.Visible = false;
            }
            if (ParametrosDGV.Columns[e.Index].HeaderText.Contains(BaseClasses._typeparam2))
            {
                PowerMillBTN.Visible = false;
                SecuenciaNumerosPanel.Visible = false;
                NombreFicherosPanel.Visible = false;
            }
            if (ParametrosDGV.Columns[e.Index].HeaderText.Contains(BaseClasses._typeparam3))
            {
                PowerMillBTN.Visible = false;
                XmlCustomFile_BTN.Visible = false;
                SecuenciaNumerosPanel.Visible = false;
            }
            if (ParametrosDGV.Columns[e.Index].HeaderText.Contains(BaseClasses._typeparam4))
            {
                PowerMillBTN.Visible = false;
                XmlCustomFile_BTN.Visible = false;
                SecuenciaNumerosPanel.Visible = false;
                NombreFicherosPanel.Visible = false;
            }
        }
        /// <summary>
        /// Nos conectamos con powermill y monstramos un arbol con todos los parametros que hemos solicitado.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PowerMillBTN_Click(object sender, EventArgs e)
        {
            OpenPowerMillOptionsPanel();
       
            ConnectToPowerMill();
            if (PowerMillisConnect)
            {
                ReadPowerMillProyect();
              
            }
        }
        /// <summary>
        /// Abrimos el panel de las distintas opciones de recojer datos del powermill
        /// </summary>
        private void OpenPowerMillOptionsPanel()
        {
            PowerMillOptionsPanel.Location = ParametrosPanel.Location;
            PowerMillOptionsPanel.Top = 380;
            PowerMillOptionsPanel.BringToFront();
        }
        /// <summary>
        /// Cerramos el panel de las distinas opciones de recojer datos del powermill
        /// </summary>
        private void ClosePowerMillOptionsPanel()
        {
            PowerMillOptionsPanel.Left = 2000;
            OpenPowerMillPanel();
        }
        /// <summary>
        /// Abrimos el Panel con el TreeView de PowerMill
        /// </summary>
        private void OpenPowerMillPanel()
        {
           // ClosePowerMillOptionsPanel();
            PowerMillPanel.Location = ParametrosPanel.Location;
            PowerMillPanel.BringToFront();
        }
        /// <summary>
        /// Cerramos el panel con el TreeView de PowerMill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClosePowerMillPanelBTN_Click(object sender, EventArgs e)
        {
            PowerMillPanel.Left = 2000;
            OnlyOneCheckItem = false;
        }
        /// <summary>
        /// Abrimos el fichero xml y lo mostramos en el treeview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XmlCustomFile_BTN_Click(object sender, EventArgs e)
        {
            //comprobamos si ya teniamos tallas cargadas
            if (xmltallas.Count <= 0)
            {
                openFileDialog1 = new OpenFileDialog();
                openFileDialog1.FileName = "";
                openFileDialog1.Filter = "All files (*.*)|*.*";//" | xml files (*.xml)|*.xml | txt files (*.txt)|*.txt";
                openFileDialog1.ShowDialog();
        
                if (!string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    xmltallas = new List<string>();
                    //leemos y almacenamos las tallas del fichero xml 
                    xmltallas = ReadCustomXMl.LoadCustomXMl(openFileDialog1.FileName);
                }
            }
            //comprobamos que existan tallas en el fichero
            if(xmltallas.Count > 0)
            {
                //mostramos el panel de las tallas
                XmlPanel.Left = ParametrosPanel.Left;
                XmlPanel.BringToFront();
                // rellenamos el treeview
                int xmltvCount = 0;
                XMLtreeView.Nodes.Clear();
                XMLtreeView.CheckBoxes = true;
                XMLtreeView.BeginUpdate();
                if (xmltallas.Count > 0)
                {
                    XMLtreeView.Nodes.Add("TALLAS");
                    for (int cnt = 0; cnt < xmltallas.Count; cnt++)
                    {
                        XMLtreeView.Nodes[xmltvCount].Nodes.Add(xmltallas[cnt]);
                        // marcamos las tallas siempre, el usuario decidira cual no quiere
                        XMLtreeView.Nodes[xmltvCount].Nodes[cnt].Checked = true;
                    }
                }
                XMLtreeView.EndUpdate();
            }
        }
        /// <summary>
        /// Recojemos los datos de las tallas seleccionadas y las insertamos en la columna de parametros seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarXmlBTN_Click(object sender, EventArgs e)
        {
            List<string> xmltallas = new List<string>();
            xmltallas = GetCheckedItemsInTree(XMLtreeView);
            InsertDataInColumParam(xmltallas);
            XmlPanelCloseBTN_Click( sender,  e);
        }
        /// <summary>
        /// Cerramos el panel con las tallas mostradas del XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XmlPanelCloseBTN_Click(object sender, EventArgs e)
        {
            XmlPanel.Left = 2000;
        }
        /// <summary>
        /// cargamos el fichero de procedimientos seleccionado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadProcedureBTN_Click(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.MediumTurquoise;

            TreeNode node = DirectoryView.SelectedNode;
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Text))
                {
                    _pathDirectory = Utils.GetRealPath(files, node.Text);
                    // MessageBox.Show(string.Format("You selected: {0}", node.Text));
                    FP_RTB.LoadFile(Path.Combine(_pathDirectory, node.Text), RichTextBoxStreamType.PlainText);
                    //limpiamos la tabla de parametros rellando anteriormente de otro procedimiento
                    CleanBTN_Click(sender, e);
                    //limpiamos el documento de los comandos generados
                    FicheroComandosTXT.Text = "";
                    //comprobamos si tenemos parametros en el fichero
                    if (CheckParamsInFile())
                    {
                        foreach (int h in Utils.AdditiveInt(690, 20))
                        {
                            this.Size = new Size(325 + h, 700);
                            this.Refresh();
                        }
                        //comprobar si solo tenemos un parametro modificable
                        if (!_param1.Contains(BaseClasses._typeparam4) && (_param2.Contains(BaseClasses._typeparam4) || string.IsNullOrEmpty(_param2)) )
                        {
                            ParamsCheck.SetItemChecked(0, true);
                            //comprobamos que tipo de parametro es
                            int TypeParam = BaseClasses.CheckTypeParam(_param1);
                            if (TypeParam != 0)
                            {
                                //si es un parametro definido abrimos el commando establecido ( ej : !mf de multifichero ejecutamos automaticamente el dialogo para seleccionar multiples ficheros
                                switch (TypeParam)
                                {
                                    case 1: //powermill 
                                        TodoPowerMillBTN_Click(sender, e);
                                        break;
                                    case 2: //xml
                                        XmlCustomFile_BTN_Click(sender, e);
                                        break;
                                    case 3: //multiples ficheros seleccion
                                        InsertaNombreFicherosBTN_Click(sender, e);
                                        break;
                                    case 4:
                                        TrayectoriasPMBTN_Click(sender, e);
                                        break;
                                    case 5:
                                        LimitesPMBTN_Click(sender, e);
                                        break;
                                    case 6:
                                        PatronesPMBTN_Click(sender, e);
                                        break;
                                    case 7:
                                        PlanosDeTrabajoPMBTN_Click(sender, e);
                                        break;
                                    case 8:
                                        ProgramasCNPMBTN_Click(sender, e);
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.Size = new Size(730, 700);
                    }
                    tabControl1.SelectedTab = tabPage1;
                }
            }
            else
            {
                if (DirectoryView.Nodes.Count > 0)
                {
                    MessageBox.Show(string.Format("Selecciona un fichero"));
                }
                else
                {
                    MessageBox.Show(string.Format("Selecciona un Directorio primero para seleccionar el fichero"));
                    DirectoySetBTN.BackColor = Color.Red;
                }

            }
        }
        /// <summary>
        /// Enviamos los comandos generados a powermill para que los ejecute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendToPowerMillBTN_Click(object sender, EventArgs e)
        {
            List<string> _cmdLines = new List<string>();
            
            foreach (string _cline in FicheroComandosTXT.Lines)
            {
                _cmdLines.Add(_cline);
            }
            ExcuteCommandsToPowerMill(_cmdLines);
        }

        private void TrayectoriasPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmToolpath);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
            isGenericName = true;
        }

        private void LimitesPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmBoundary);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
            isGenericName = true;
        }

        private void PatronesPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmPattern);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
            isGenericName = true;
        }

        private void HerramientasPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmTool);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
        }

        private void PlanosDeTrabajoPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmWorkplane);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
        }

        private void ProgramasCNPMBTN_Click(object sender, EventArgs e)
        {
            CreateCustomPowerMillTreeView(clsPowerMILLOLE.enumPowerMILLEntityType.pmNCProgram);
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
            isGenericName = true;
        }

        private void TodoPowerMillBTN_Click(object sender, EventArgs e)
        {
            CreatePowerMillTreeView();
            ClosePowerMillOptionsPanel();
            OnlyOneCheckItem = true;
        }

        private void ClosePMOptionsBTN_Click(object sender, EventArgs e)
        {
            ClosePowerMillOptionsPanel();
        }

        private void Main_Leave(object sender, EventArgs e)
        {
            try
            {
                PowerMill.CloseOLEConnections();
            }
            catch (Exception ex) { }
        }
        //Es para que solo se seleccione un elemento cuando se lo indiquemos
        private void PowerMillTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (OnlyOneCheckItem)
            {
                // only do it if the node became checked:
                if (e.Node.Checked)
                {
                    // for all the nodes in the tree...
                    foreach (TreeNode cur_node in e.Node.TreeView.Nodes)
                    {
                        // ... which are not the freshly checked one...
                        if (cur_node != e.Node)
                        {
                            // ... uncheck them
                            cur_node.Checked = false;
                        }
                        foreach (TreeNode sub_node in cur_node.Nodes)
                        {
                            if (sub_node != e.Node)
                            {
                                // ... uncheck them
                                sub_node.Checked = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
