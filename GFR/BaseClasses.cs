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
        public const string _typeparam4_1 = "!trpm";
        public const string _typeparam5 = "!lipm";
        public const string _typeparam6 = "!prpm";
        public const string _typeparam7 = "!hrpm";
        public const string _typeparam8 = "!ptpm";
        public const string _typeparam9 = "!cnpm";


        public static int CheckTypeParam(string _param)
        {
            if (_param.Contains(_typeparam1))
            {
                return 1;
            }
            if (_param.Contains(_typeparam2))
            {
                return 2;
            }
            if (_param.Contains(_typeparam3))
            {
                return 3;
            }
            if (_param.Contains(_typeparam4_1))
            {
                return 4;
            }
            if (_param.Contains(_typeparam5))
            {
                return 5;
            }
            if (_param.Contains(_typeparam6))
            {
                return 6;
            }
            if (_param.Contains(_typeparam7))
            {
                return 7;
            }
            if (_param.Contains(_typeparam8))
            {
                return 8;
            }
            return 0;
        }

    }
    /// <summary>
    /// Clase base para los parametros de la cabezera del fichero
    /// </summary>
    public class BaseHeadParams
    {
        string _varName;
        string _varValue;
        string _varParm;
        bool _isPath = false;

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
        public bool isPath 
        { 
            get { return _isPath; } 
            set { _isPath = value; } 
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
            if (_line.Contains(BaseClasses._headformat3)) isPath = true;
            
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
        System.Windows.Forms.Button _button = new System.Windows.Forms.Button();
        bool _havePath = false;

        string _paramName="";

        public System.Windows.Forms.Label LabelNameCC {
            get { return _labelName; }
            set { _labelName = value; }
        }
        public System.Windows.Forms.TextBox TextBoxCC {
            get { return _textBox;}
            set { _textBox = value;}
        }
        /// <summary>
        /// se usa unicamente para añadir la funcion de elegir directorio
        /// </summary>
        public System.Windows.Forms.Button ButtonCC
        {
            get { return _button; }
            set { _button = value; }
        }
        public string ParamName
        {
            get { return _paramName; }
            set { _paramName = value; }
        }
        public bool HavePath 
        { 
            get { return _havePath; }
            set { _havePath = value; } 
        }

        public CustomControls(System.Windows.Forms.TextBox _tb, System.Windows.Forms.Label _ln,string _p)
        {
            _textBox = _tb;
            _labelName = _ln;
            _paramName = _p;
            _havePath = false;
        }

        public CustomControls(System.Windows.Forms.TextBox _tb, System.Windows.Forms.Label _ln, string _p, System.Windows.Forms.Button _btn)
        {
            _textBox = _tb;
            _labelName = _ln;
            _paramName = _p;
            _button = _btn;
            _havePath = true;
        }
   
    }

    public static class MyCustomExtensions
    {
        /// <summary>
        /// Comprobamos que tengamos datos en todos los texbox
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static bool CheckValuesOK(this List<CustomControls> cc)
        {
            foreach (CustomControls _cc in cc)
            {
                if (string.IsNullOrEmpty(_cc.TextBoxCC.Text) || string.IsNullOrWhiteSpace(_cc.LabelNameCC.Text)) return false;
            }
            return true;
        }
        /// <summary>
        /// metodo que devuelve el nombre generico del mismo
        /// </summary>
        /// <param name="_name">nombre el cual queremos generizar si termina en _xxx donde xxx sera un numero en un string</param>
        /// <returns></returns>
        public static string GetGenericName(this string _name)
        {
            string auxname = _name;
            string numToRemove = "";
            string[] _ln = _name.Split('_');
            foreach (string s in _ln)
            {
                int num = 0;
                if (int.TryParse(s, out num))
                {
                    numToRemove = s;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(numToRemove))
            {
                auxname = _name.Replace("_" + numToRemove, "");
            }
       
           return auxname;
        }
        /// <summary>
        /// Graba todos los parametros de la cabezera en un fichero
        /// </summary>
        /// <param name="_bhpList"></param>
        public static void SaveHeadParamsInFile(this List<BaseHeadParams> _bhpList)
        {
            Ini.IniFile _myIni = new Ini.IniFile(".\\headvars.ini");
            foreach (BaseHeadParams _bp in _bhpList)
            {
                _myIni.IniWriteValue("HeadParams", _bp.varParam, _bp.varValue);
            }
        }
        /// <summary>
        /// añade los parametros de cabezera del actual prodecimiento que existan en el fichero de los parametros
        /// </summary>
        /// <param name="_bhpList"></param>
        public static void GetHeadParamsFromFile(this List<BaseHeadParams> _bhpList)
        {
            Ini.IniFile _myIni = new Ini.IniFile(".\\headvars.ini");
            foreach (BaseHeadParams _bp in _bhpList)
            {
                string _var = _myIni.IniReadValue("HeadParams", _bp.varParam);
                if (!string.IsNullOrEmpty(_var))
                {
                    foreach (CustomControls cc in Utils.myCustomControls)
                    {
                        if (String.Equals(cc.ParamName, _bp.varParam))
                        {
                             cc.TextBoxCC.Text = _var;
                        }
                    }
                }
            }
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

    public class PowerMillData
    {
        public PowerSolutionDOTNetOLE.clsPowerMILLOLE.enumPowerMILLEntityType nameType;
        public int count;
        public int ID;
        public string name;
        //public List<string> Data = new List<string>();
        public string[] Data;
        public PowerMillData()
        {
            nameType = PowerSolutionDOTNetOLE.clsPowerMILLOLE.enumPowerMILLEntityType.pmBoundary;
            count = 0;
            ID = 0;
           
        }

        public PowerMillData(PowerSolutionDOTNetOLE.clsPowerMILLOLE.enumPowerMILLEntityType _nameType, string _name)
        {
            name = _name;
            nameType = _nameType;
            count = 0;
            ID = 0;
        }
    }
}
