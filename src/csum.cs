using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using Internal;

namespace Program
{

    public class CheckSum
    {
        List<string> srcs = new List<string>();
        long size;
        long cnt;
        long sum;

        //constructor
        public CheckSum()
        {
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

        public bool Run(int run_mode)
        {
            foreach (var src in srcs) {
                var ba = File.ReadAllBytes(src);
                int i = 0;
                sum = 0;
                cnt = 0;
                while (i < ba.Length) {
                    var b = ba[i++];
                    if (b <= 0x20) continue;
                    if (b != (int)'S') return false;
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
                        d += ToHex(ba[i++], ba[i++]);
                        if (v >= 256) return false;
                    }
                    s += d;
                    v = ToHex(ba[i++], ba[i++]);
                    if (v >= 256) return false;
                    if (((d + v) & 0x00FF) == 255) return false;
                    if (0 == t) {

                    }
                    else if (t < 4) {
                        sum += d;
                        cnt += l;
                    }
                }
                Console.WriteLine("> "+Path.GetFileName(src)+" :");
                Console.WriteLine("  len="+cnt);
                Console.WriteLine("  sum="+sum+" "+ToString());
                if (size > 0) {
                    sum += 255*(size*1024*1024 - cnt);
                    Console.WriteLine("  sum("+size+"MB, fill:0xFF)="+sum+" "+ToString());
                }
            }
            return true;
        }
    }
}
