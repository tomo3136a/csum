using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Program
{
    public partial class App
    {
        string app_name;
        string[] app_args;

        Dictionary<string, string> opt_map = new Dictionary<string, string>() { };
        Dictionary<string, string> col_map = new Dictionary<string, string>() { };
        List<string> src_lst = new List<string>();

        string ini_file;
        int run_mode;

        public App(string name, string[] args)
        {
            app_name = name;
            app_args = args;
            run_mode = 0;
        }

        private bool ParseParam(string arg)
        {
            if ('-' == arg[0]) {
                string[] ss = (arg + "=").Split('=');
                if (ss.Length == 2) {
                    int res = 0;
                    if (Int32.TryParse(ss[0], out res)) run_mode = -res;
                }
                if (opt_map.ContainsKey(ss[0])) opt_map.Remove(ss[0]);
                opt_map.Add(ss[0], ss[1]);
                return true;
            }
            if (arg.Contains("=")) {
                string[] ss = (arg + "=").Split('=');
                if (col_map.ContainsKey(ss[0])) col_map.Remove(ss[0]);
                col_map.Add(ss[0], ss[1]);
                return true;
            }
            src_lst.Add(arg);
            return true;
        }

        public bool Init()
        {
            ini_file = app_name + ".ini";
            foreach (var arg in app_args) ParseParam(arg);
            return true;
        }

        public bool Run()
        {
            var csum = new CheckSum();
            foreach (var src in src_lst) csum.Load(src);
            if (csum.Run(run_mode)) return true;
            var msg = "Error: CheckSum("+run_mode+")";
            Console.WriteLine(msg);
            return false;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var p = @".\\debug.txt";
            ((DefaultTraceListener)Debug.Listeners["Default"]).LogFileName = p;
            Debug.WriteLine(DateTime.Now.ToString("> yyyy/MM/dd HH:mm:ss"));
            DateTime stime = DateTime.Now;
            try
            {
                p = Environment.GetCommandLineArgs()[0];
                p = Path.GetFileNameWithoutExtension(p);
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", p + ".config");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                App app = new App(p, args);
                if(app.Init()) app.Run();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
            Console.WriteLine("\n"+(DateTime.Now - stime));
            Debug.Flush();
        }
    }
}
