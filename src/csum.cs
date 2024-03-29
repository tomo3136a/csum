#define TRACE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Program
{
    public class CheckSum
    {
        List<string> srcs = new List<string>();
        Encoding enc;
        long size;
        long cnt;
        long sum;

        //constructor
        public CheckSum()
        {
            enc = Encoding.ASCII;
        }

        public void Load(string src)
        {
            srcs.Add(src);
        }

        /////////////////////////////////////////////////////////////////////

        //accessor
        public long  Size { get{ return size; } set{ size = value; } }
        public long  Count { get{ return cnt; } }
        public long  Value { get{ return sum; } }
        public Encoding Encoding { get { return enc; } set { enc = value; } }
        public override string ToString() { return String.Format("{0:X8}", sum); }

        const int MB = 256;
        static int[] HEX = {
            MB, 10, 11, 12, 13, 14, 15, MB, MB, MB, MB, MB, MB, MB, MB, MB, 
            0,  1,  2,  3,  4,  5,  6,  7,  8,  9,  MB, MB, MB, MB, MB, MB
        };
        static int[] AL = { 2, 2, 3, 4, 0, 2, 3, 4, 3, 2 };
        public static int ToHex(byte h, byte l)
        {
            return (HEX[h & 0x1F] << 4) + HEX[l & 0x1F];
        }

        public static string ParseToString(byte[] ba, long sp, long ep, Encoding enc=null)
        {
            var l = (ep - sp) / 2;
            var a = new byte[l];
            var i = sp;
            var j = 0;
            for (j = 0; j < l; j ++) {
                var v = ToHex(ba[i++], ba[i++]);
                if (v == 0) break;
                a[j] = (byte)v;
            }
            if (enc == null) enc = Encoding.ASCII;
            return enc.GetString(a, 0, j);
        }

        public bool Config(Dictionary<string, string> dict) {
            var res = false;
            if (dict.ContainsKey("-size")) {
                Size = Int32.Parse("0"+dict["-size"]);
                res = true;
            }
            if (dict.ContainsKey("-enc")) {
                enc = System.Text.Encoding.GetEncoding(dict["-enc"]);
                res = true;
            }
            return res;
        }

        public bool Run(int run_mode)
        {
            foreach (var src in srcs) {
                var now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                if (!Calc(src)) return false;
                var name = Path.GetFileName(src);
                var size = (0 == Size) ? "" : (" [" + Size + "MB]");
                var data = ToString().Substring(ToString().Length - 8);
                var fmt = "# {0} {1} {2}{3}";
                var msg = String.Format(fmt, now, data, name, size);
                Trace.WriteLine(msg);
                Console.WriteLine("");
            }
            return true;
        }

        public bool Calc(string src)
        {
            var ba = File.ReadAllBytes(src);
            sum = 0;
            cnt = 0;
            Console.WriteLine("file name   : " + Path.GetFileName(src));
            Console.WriteLine("file size   : " + ba.Length);
            int line = 0;
            int i = 0;
            bool eol = false;
            while (i < ba.Length) {
                var b = ba[i++];
                if (b <= 0x20) {
                    if (b == 0x0d || b == 0x0a) eol = true;
                    continue;
                }
                if (b != (int)'S') return false;
                eol = false;
                var t = ba[i++] & 0x0F;
                if (t > 9) return false;
                var v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                var l = v;
                var s = v;
                var a = 0;
                var al = AL[t];
                for (var j = 0; j < al; j ++) {
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    a = (a << 8) + v;
                    s += v;
                }
                var d = 0;
                l -= al + 1;
                for (var k = 0; k < l; k ++) {
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    d += v;
                }
                s += d;
                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                s += v;
                if ((s & 0x00FF) != 255) {
                    var msg = "record checksum ";
                    WriteErrorLine(msg + line + ".");
                    return false;
                }
                if (0 == t) {
                    var note = ParseToString(ba, i - 2 * l - 2, i - 2, enc);
                    Console.WriteLine("note        : " + note);
                }
                else if (t < 4) {
                    line ++;
                    sum += d;
                    cnt += l;
                }
                else if (t < 5) {
                    Console.WriteLine("#error not support S" + t);
                }
                else if (t < 7) {
                    Console.WriteLine("S" + t + " record   : " + a);
                    if (line != a) {
                        var msg = "no match record count ";
                        WriteErrorLine(msg + line + ".");
                    }
                }
                else {
                    var fmt = "{0:X" + (AL[t] * 2) + "}";
                    var str = String.Format(fmt, a);
                    Console.WriteLine("entry point : " + str);
                }
            }
            if (!eol) {
                var msg = "EOF is not blank.";
                WriteErrorLine(msg);
            }
            Console.WriteLine("record count: " + line);
            Console.WriteLine("byte count  : " + cnt);
            if (size > 0) {
                Console.Write("rom size    : " + size + "MB");
                Console.WriteLine(" (fill=0xFF)");
                sum += 255 * (size * 1024 * 1024 - cnt);
            }
            Console.WriteLine("sum         : " + sum);
            Console.WriteLine("sum (hex)   : " + ToString());
            return true;
        }

        void WriteErrorLine(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write("#error: " + msg);
            Console.ResetColor();
            Console.WriteLine("");
        }
    }
}
