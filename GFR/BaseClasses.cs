using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GFR
{
    public class BaseClasses
    {
        //aqui algo en un futuro para desglosar cosas basicas
        public const string _headformat1 = "##";
        public const string _headformat2 = "< >";
        public const string _headformat3 = "<< >>";
        public const string _typeparam1 = "!pm";
        public const string _typeparam2 = "!xml";
        public const string _typeparam3 = "!mf";
        public const string _typeparam4 = "_path";
    }
    /// <summary>
    /// Clase base para los parametros de la cabezera del fichero
    /// </summary>
    public class BaseHeadParams
    {
        string _varName;
        string _varValue;
        string _varParm;

        #region GETHERS AND SETTHERS
        public string varValue
        {
            get { return _varValue;}
            set { _varValue = value;}
        }
        public string varName
        {
            get { return _varName; }
            set { _varName = value; }
        }
        public string varParam
        {
            get { return _varParm; }
            set { _varParm = value; }
        }
        #endregion

        #region CONSTRUCTORS
        public BaseHeadParams(string n,string v)
        {
            _varName = n;
            _varValue = v;
        }
        public BaseHeadParams(string _line)
        {
            SetParamsFromLine(_line);
        }
        #endregion

        public void SetParamsFromLine(string _line)
        {
            string auxline2 = _line.Replace(BaseClasses._headformat1, "");
            string auxline = auxline2.Replace(BaseClasses._headformat2, "");
            string auxName = "";
            string auxValue = "";
            string auxParam = "";
            bool isNameSet = false;
            foreach (char c in auxline)
            {
                if (c == ';')
                {
                    isNameSet = true;
                }
                else
                {
                    if (c != '<' && c != '>')
                    {
                        if (isNameSet)
                        {
                            auxParam += c;
                        }
                        else
                        {
                            auxName += c;
                        }
                    }
                }
            }
            _varName = auxName;
            _varParm = auxParam;
            _varValue = auxValue;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CustomControls
    {
        System.Windows.Forms.TextBox _textBox = new System.Windows.Forms.TextBox();
        System.Windows.Forms.Label _labelName = new System.Windows.Forms.Label();
        string _paramName="";

        public System.Windows.Forms.Label LabelNameCC {
            get { return _labelName; }
            set { _labelName = value; }
        }
        public System.Windows.Forms.TextBox TextBoxCC {
            get { return _textBox;}
            set { _textBox = value;}
        }
        public string ParamName
        {
            get { return _paramName; }
            set { _paramName = value; }
        }

        public CustomControls(System.Windows.Forms.TextBox _tb, System.Windows.Forms.Label _ln,string _p)
        {
            _textBox = _tb;
            _labelName = _ln;
            _paramName = _p;
        }
        /// <summary>
        /// Comprobamos que tengamos datos en todos los texbox
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
    
    }

    public static class MyCustomExtensions
    {
        public static bool CheckValuesOK(this List<CustomControls> cc)
        {
            foreach (CustomControls _cc in cc)
            {
                if (string.IsNullOrEmpty(_cc.TextBoxCC.Text) || string.IsNullOrWhiteSpace(_cc.LabelNameCC.Text)) return false;
            }
            return true;
        }
    }

    public static class ReadCustomXMl
    {
        static XmlDocument xDocXML = new XmlDocument();
        public static List<string> LoadCustomXMl(string _file)
        {
            xDocXML.Load(_file);
            XmlNodeList perstanya = xDocXML.GetElementsByTagName("Pestanya1");
            XmlNodeList listaTallas = ((XmlElement)perstanya[0]).GetElementsByTagName("listaTallas");
            XmlNodeList nTalla = null;
            List<string> xTallas = new List<string>();
            
            foreach (XmlElement nodo in listaTallas)
            {
                 nTalla =  nodo.GetElementsByTagName("string");
            }
            if (nTalla != null)
            {
                foreach (XmlNode n in nTalla)
                {
                    string at = n.InnerText;
                    string[] att = at.Split('|');
                    
                    xTallas.Add(att[0].Trim());
                }
            }
            return xTallas;
        }
    }
}
