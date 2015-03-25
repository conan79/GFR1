using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace GFR
{
    public class Utils
    {
        public static List<CustomControls> myCustomControls = new List<CustomControls>();
        
        /// <summary>
        /// Funcion que devuelve la ruta de un fichero 
        /// </summary>
        /// <param name="_paths"></param>
        /// <param name="_file"></param>
        /// <returns></returns>
        static public string GetRealPath(string[] _paths,string _file)
        {
            string realPath="";
            foreach (string f in _paths)
            {
                if (Path.GetFileName(f) == _file)
                {
                    realPath = Path.GetDirectoryName(f);
                }
            }
            return realPath;
        }
        /// <summary>
        /// incremento exponencial con ligera pausa
        /// </summary>
        /// <param name="num">cuanto queremos añadir</param>
        /// <param name="amount">en que medida añadimos</param>
        /// <returns></returns>
        static public IEnumerable<int> AdditiveInt(int num, int amount)
        {
            int result = 0;
            while (result <= num)
            {
                result += amount;
                if (result > num) result = num+1;
                yield return result;
            }
          
        }

        static public void CreateLabelEdit(object sender,System.Collections.Generic.List<BaseHeadParams> bhp)
        {
            int TopOffset = 15;
            int amount;
            amount = bhp.Count;
            if (amount > 0)
            {
                //creamos tantos objetos como numero parametros para tener la base de construccion
                TextBox[] textBoxes = new TextBox[amount];
                Label[] labels = new Label[amount];
                Button[] buttons = new Button[amount];

                //borrar del panel los anteriores controles
                foreach (CustomControls cc in myCustomControls)
                {
                    (sender as Panel).Controls.Remove(cc.LabelNameCC);
                    cc.LabelNameCC.Dispose();
                    (sender as Panel).Controls.Remove(cc.TextBoxCC);
                    cc.TextBoxCC.Dispose();
                    (sender as Button).Controls.Remove(cc.ButtonCC);
                    cc.ButtonCC.Dispose();

                }

                myCustomControls.Clear();

                for (int i = 0; i < amount; i++)
                {
                    textBoxes[i] = new TextBox();
                    buttons[i] = new Button();
                    // Here you can modify the value of the textbox which is at textBoxes[i]
                    textBoxes[i].Left = 180;
                    textBoxes[i].Top = TopOffset + (25 * i);
                    textBoxes[i].Name = "cctxt" + i.ToString();
                    labels[i] = new Label();
                    labels[i].Width = 200;
                    labels[i].Left = 15;
                    labels[i].Top = TopOffset + (25 * i);
                    labels[i].Text = bhp[i].varName;
                    labels[i].Name = "cclbl" + i.ToString();
                    //si es de tipo 3 <<>> añadiremos un botton para cojer la ruta
                    if (bhp[i].isPath)
                    {
                        buttons[i].Name = "ccbtn" + i.ToString();
                        buttons[i].Width = 25;
                        buttons[i].Height = 25;
                        buttons[i].Text = "D";
                        buttons[i].BackColor = System.Drawing.Color.LightGoldenrodYellow;
                        buttons[i].Top = textBoxes[i].Top;
                        buttons[i].Left = 200 + textBoxes[i].Width;
                        buttons[i].Click += new System.EventHandler(test1);
                        myCustomControls.Add(new CustomControls(textBoxes[i], labels[i], bhp[i].varParam,buttons[i]));
                    }
                    else
                    {
                        myCustomControls.Add(new CustomControls(textBoxes[i], labels[i], bhp[i].varParam));
                    }
                }

                // esto añade los controles al form
                foreach (CustomControls cc in myCustomControls)
                {
                    (sender as Panel).Controls.Add(cc.TextBoxCC);
                    (sender as Panel).Controls.Add(cc.LabelNameCC);
                    if (cc.HavePath)
                    {

                        (sender as Panel).Controls.Add(cc.ButtonCC);
                   
                    }
                }
            }
        }

        static void test1(object sender,System.EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            DialogResult result = FBD.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (CustomControls cc in myCustomControls)
                {
                    if (cc.ButtonCC.Name == (sender as Button).Name)
                    {
                        cc.TextBoxCC.Text = FBD.SelectedPath +"\\" ;
                    }
                }
               
            }
        }

      
    }
}
