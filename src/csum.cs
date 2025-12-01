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
        private List<string> _srcs = new List<string>();
        private Encoding _enc = Encoding.ASCII;

        public enum FmtType
        {
            AUTO, SREC, IHEX, BIN
        }
        private FmtType _type = FmtType.AUTO;

        private long _size = 0;
        private long _cnt = 0;
        private long _sum = 0;

        private int _out_size = 0;
        private int _out_cnt = 0;
        private int _out_sum = 0;

        /////////////////////////////////////////////////////////////////////
        // constructor

        public CheckSum()
        {
        }

        /// <summary>
        /// Add load file
        /// </summary>
        /// <param name="src">source path</param>
        public void Load(string src)
        {
            _srcs.Add(src);
        }

        /////////////////////////////////////////////////////////////////////
        // accessor

        public FmtType Type { get { return _type; } set { _type = value; } }
        public long Size { get { return _size; } set { _size = value; } }
        public long Count { get { return _cnt; } }
        public long Value { get { return _sum; } }
        public Encoding Encoding { get { return _enc; } set { _enc = value; } }
        public int OutSizeFile { get { return _out_size; } set { _out_size = value; } }
        public int OutCountFile { get { return _out_cnt; } set { _out_cnt = value; } }
        public int OutSumFile { get { return _out_sum; } set { _out_sum = value; } }
        public override string ToString() { return String.Format("{0:X8}", _sum); }

        /////////////////////////////////////////////////////////////////////
        // common function

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

        public static string ParseToString(byte[] ba, long sp, long ep, Encoding enc = null)
        {
            var l = (ep - sp) / 2;
            var a = new byte[l];
            var i = sp;
            var j = 0;
            for (j = 0; j < l; j++)
            {
                var v = ToHex(ba[i++], ba[i++]);
                if (v == 0) break;
                a[j] = (byte)v;
            }
            if (enc == null) enc = Encoding.ASCII;
            return enc.GetString(a, 0, j);
        }

        public bool Config(Dictionary<string, string> dict)
        {
            var res = false;
            if (dict.ContainsKey("-type"))
            {
                var s = dict["-type"].ToLower();
                if (s != "")
                {
                    switch (s[0])
                    {
                        case 's': Type = FmtType.SREC; break;
                        case 'i': Type = FmtType.IHEX; break;
                        case 'b': Type = FmtType.BIN; break;
                        default: Type = FmtType.AUTO; break;
                    }
                }
                res = true;
            }
            if (dict.ContainsKey("-size"))
            {
                Size = Int32.Parse("0" + dict["-size"]);
                res = true;
            }
            if (dict.ContainsKey("-enc"))
            {
                _enc = System.Text.Encoding.GetEncoding(dict["-enc"]);
                res = true;
            }
            if (dict.ContainsKey("-out"))
            {
                foreach (var s in dict["-out"].Split(new char[] { ',' }))
                {
                    var ss = s.Split(new char[] { ':' });
                    var n = (ss.Length > 1) ? Int32.Parse("0" + ss[1]) : 8;
                    var k = ss[0];
                    if (k.Length < 1) continue;
                    var h = 0;
                    if (k[k.Length - 1] == '1') { h = 1; k = k.Substring(0, k.Length - 1); }
                    if (k[k.Length - 1] == '2') { h = 2; k = k.Substring(0, k.Length - 1); }
                    n += 100 * h;
                    switch (k.ToLower())
                    {
                        case "size": OutSizeFile = n; break;
                        case "count": OutCountFile = n; break;
                        case "sum": OutSumFile = n; break;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// run
        /// </summary>
        /// <param name="run_mode"></param>
        /// <returns></returns>
        public bool Run(int run_mode)
        {
            foreach (var src in _srcs)
            {
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

        /// <summary>
        /// sum check calcuration
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public bool Calc(string src)
        {
            var ba = File.ReadAllBytes(src);
            _sum = 0;
            _cnt = 0;
            Console.WriteLine("file name   : " + Path.GetFileName(src));
            Console.WriteLine("file size   : " + ba.Length);
            switch (_type)
            {
                case FmtType.AUTO:
                case FmtType.SREC:
                    if (!CalcSRec(ba)) return false;
                    break;
                case FmtType.IHEX:
                    if (!CalcIHex(ba)) return false;
                    break;
                case FmtType.BIN:
                    if (!CalcBin(ba)) return false;
                    break;
            }
            Console.WriteLine("byte count  : " + _cnt);
            if (_size > 0)
            {
                Console.Write("rom size    : " + _size + "MB");
                Console.WriteLine(" (fill=0xFF)");
                _sum += 255 * (_size * 1024 * 1024 - _cnt);
            }
            Console.WriteLine("sum         : " + _sum);
            Console.WriteLine("sum (hex)   : " + ToString());
            if (_out_size > 0) OutBinary("size.bin", _size, _out_size);
            if (_out_cnt > 0) OutBinary("count.bin", _cnt, _out_cnt);
            if (_out_sum > 0) OutBinary("sum.bin", _sum, _out_sum);
            return true;
        }

        /// <summary>
        /// out of result binary
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <param name="opt"></param>
        public void OutBinary(string f, long v, int opt)
        {
            var h = opt / 100;
            var n = opt % 100;
            if (h == 1) v = ~v;
            if (h == 2) v = -v;
            using (var fs = new FileStream(f, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    switch (n)
                    {
                        case 1: bw.Write((char)(v & 0x0FF)); break;
                        case 2: bw.Write((short)(v & 0x0FFFF)); break;
                        case 4: bw.Write((int)(v & 0x0FFFFFFFF)); break;
                        default: bw.Write(v); break;
                    }
                }
            }
        }

        /// <summary>
        /// calcurate S record 
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        public bool CalcSRec(byte[] ba)
        {
            int line = 0;
            int i = 0;
            bool eol = false;
            Console.WriteLine("file format : S record");
            while (i < ba.Length)
            {
                var b = ba[i++];
                if (b <= 0x20)
                {
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
                for (var j = 0; j < al; j++)
                {
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    a = (a << 8) + v;
                    s += v;
                }
                var d = 0;
                l -= al + 1;
                for (var k = 0; k < l; k++)
                {
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    d += v;
                }
                s += d;
                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                s += v;
                if ((s & 0x00FF) != 255)
                {
                    var msg = "record checksum ";
                    WriteErrorLine(msg + line + ".");
                    return false;
                }
                if (0 == t)
                {
                    var note = ParseToString(ba, i - 2 * l - 2, i - 2, _enc);
                    Console.WriteLine("note        : " + note);
                }
                else if (t < 4)
                {
                    line++;
                    _sum += d;
                    _cnt += l;
                }
                else if (t < 5)
                {
                    Console.WriteLine("#error not support S" + t);
                }
                else if (t < 7)
                {
                    Console.WriteLine("S" + t + " record   : " + a);
                    if (line != a)
                    {
                        var msg = "no match record count ";
                        WriteErrorLine(msg + line + ".");
                    }
                }
                else
                {
                    var fmt = "{0:X" + (AL[t] * 2) + "}";
                    var str = String.Format(fmt, a);
                    Console.WriteLine("entry point : " + str);
                }
            }
            if (!eol)
            {
                var msg = "EOF is not blank.";
                WriteErrorLine(msg);
            }
            Console.WriteLine("record count: " + line);
            return true;
        }

        /// <summary>
        /// calcurate IHex record 
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        public bool CalcIHex(byte[] ba)
        {
            int line = 0;
            int i = 0;
            bool eol = false;
            Console.WriteLine("file format : IHex record");
            while (i < ba.Length)
            {
                var b = ba[i++];
                if (b <= 0x20)
                {
                    if (b == 0x0d || b == 0x0a) eol = true;
                    continue;
                }
                if (b != (int)':') return false;
                eol = false;

                var v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                var l = v;
                var s = v;

                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                var a = v;
                s += v;
                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                a = (a << 8) + v;
                s += v;

                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                var t = v;
                if (t > 5) return false;
                s += v;

                var d = 0;
                var e = 0;
                for (var k = 0; k < l; k++)
                {
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    d += v;
                    e = (e << 8) + v;
                }
                s += d;

                v = ToHex(ba[i++], ba[i++]);
                if (v >= 256) return false;
                s += v;
                if ((s & 0x00FF) != 0)
                {
                    var msg = "record checksum " + s;
                    WriteErrorLine(msg + line + ".");
                    return false;
                }

                if (t == 0)
                {
                    line++;
                    _sum += d;
                    _cnt += l;
                }
                else if (t == 1)
                {
                    Console.WriteLine("end of file : " + line);
                }
                else if (t == 2)
                {
                    var fmt = "{0:X5}";
                    var str = String.Format(fmt, e * 16);
                    Console.WriteLine("base address: " + str);
                }
                else if (t == 4)
                {
                    var fmt = "{0:X8}";
                    var str = String.Format(fmt, e * 65536);
                    Console.WriteLine("base address: " + str);
                }
                else if (t == 3 || t == 5)
                {
                    var fmt = "{0:X8}";
                    var str = String.Format(fmt, e);
                    Console.WriteLine("entry point : " + str);
                }
            }
            if (!eol)
            {
                var msg = "EOF is not blank.";
                WriteErrorLine(msg);
            }
            Console.WriteLine("record count: " + line);
            return true;
        }

        /// <summary>
        /// calcurate binasy
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        public bool CalcBin(byte[] ba)
        {
            int i = 0;
            Console.WriteLine("file format : binary");
            while (i < ba.Length)
            {
                var b = ba[i++];
                _sum += 0x0FF & b;
                _cnt++;
            }
            return true;
        }

        /// <summary>
        /// print error message
        /// </summary>
        /// <param name="msg"></param>
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
