#define TRACE

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;

namespace Program
{
    public partial class App
    {
        string _app_name = "";
        string[] _app_args = new string[] { };

        Dictionary<string, string> _opt_map = new Dictionary<string, string>() { };
        Dictionary<string, string> _col_map = new Dictionary<string, string>() { };
        List<string> _src_lst = new List<string>();

        string _ini_file = "";
        int _run_mode = 0;

        /// <summary>
        /// application consstructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public App(string name, string[] args)
        {
            _app_name = name;
            _app_args = args;
            _run_mode = 0;
        }

        /// <summary>
        /// parse of commandline
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool ParseCommandLine(string arg)
        {
            if ('-' == arg[0])
            {
                string[] ss = (arg + "=").Split('=');

                //run-level(-n)
                if (ss.Length == 2)
                {
                    int res = 0;
                    if (Int32.TryParse(ss[0], out res)) _run_mode = -res;
                }

                // option(-xxx=yyyy)
                if (_opt_map.ContainsKey(ss[0])) _opt_map.Remove(ss[0]);
                _opt_map.Add(ss[0], ss[1]);
                return true;
            }

            // condition(xxx=yyyy)
            if (arg.Contains("="))
            {
                string[] ss = (arg + "=").Split('=');
                if (_col_map.ContainsKey(ss[0])) _col_map.Remove(ss[0]);
                _col_map.Add(ss[0], ss[1]);
                return true;
            }

            // source
            _src_lst.Add(arg);
            return true;
        }

        /// <summary>
        /// initialize
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            _ini_file = _app_name + ".ini";
            foreach (var arg in _app_args) ParseCommandLine(arg);
            return true;
        }

        /// <summary>
        /// main task run
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            var csum = new CheckSum();
            csum.Config(_opt_map);
            foreach (var src in _src_lst) csum.Load(src);
            if (csum.Run(_run_mode)) return true;
            var msg = "Error: CheckSum(" + _run_mode + ")";
            Console.WriteLine(msg);
            return false;
        }

        /// <summary>
        /// main function
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(string[] args)
        {
            DateTime stime = DateTime.Now;
            try
            {
                var s = Environment.GetCommandLineArgs()[0];
                var name = Path.GetFileNameWithoutExtension(s);

                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", name + ".config");
                var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var data = conf.AppSettings.Settings["log"];
                var log = (null == data) ? (name + ".log") : data.Value;
                Trace.Listeners.Add(new TextWriterTraceListener(log, "myListener"));

                // Application.EnableVisualStyles();
                // Application.SetCompatibleTextRenderingDefault(false);
                s = DateTime.Now.ToString("# yyyy/MM/dd HH:mm:ss");
                Console.WriteLine(s);

                App app = new App(name, args);
                if (app.Init()) app.Run();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
            Console.WriteLine("# " + (DateTime.Now - stime));
            Console.WriteLine("");
            Trace.Flush();
        }
    }
}
