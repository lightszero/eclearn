using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test001
{
    public class ECElement
    {
        public byte index;
        public byte[] block;
        public List<byte[]> byteecc;
        public byte[] ToBytes()
        {
            var data = new byte[byteecc.Count * byteecc[0].Length + block.Length + 3];
            data[0] = (byte)index;
            data[1] = (byte)byteecc[0].Length;
            data[2] = (byte)byteecc.Count;
            int seek = 3;
            for (var i = 0; i < byteecc.Count; i++)
            {
                for (var j = 0; j < byteecc[0].Length; j++)
                {
                    data[seek] = byteecc[i][j];
                    seek++;
                }
            }
            for (var i = 0; i < block.Length; i++)
            {
                data[seek] = block[i];
                seek++;
            }
            return data;
        }
        public void FromBytes(byte[] data)
        {
            this.byteecc = new List<byte[]>();
            int seek = 3;
            this.index = data[0];
            var ecclen = data[1];
            var ecccount = data[2];
            for (var i = 0; i < ecccount; i++)
            {
                var item = new byte[ecclen];
                for (var j = 0; j < item.Length; j++)
                {
                    item[j] = data[seek];
                    seek++;
                }
                this.byteecc.Add(item);
            }
            var blocklen = data.Length - seek;
            this.block = new byte[blocklen];
            for (var i = 0; i < blocklen; i++)
            {
                block[i] = data[seek];
                seek++;
            }

        }
        public override string ToString()
        {
            var str = "";
            foreach (var b in ToBytes())
            {
                str += b.ToString("X02");
            }
            return str;
        }
    }
    public class ECSplit
    {
        public static void SplitBytes(byte[] srcdata, List<byte[]> outdata, int blocksize)
        {
            var blockcount = srcdata.Length / blocksize;
            if (srcdata.Length % blocksize != 0)
                blockcount++;
            for (var i = 0; i < blockcount; i++)
            {
                byte[] data = new byte[blocksize];
                for (var j = 0; j < blocksize; j++)
                {
                    var srcindex = blocksize * i + j;
                    if (srcindex < srcdata.Length)
                    {
                        data[j] = srcdata[srcindex];
                    }
                    else
                    {
                        data[j] = 0;
                    }
                }
                outdata.Add(data);
            }
        }
        public static ECElement[] SplitDataWithBFTN(byte[] srcdata, int N = 1)
        {
            var totallen = 3 * N + 1;
            var checkecc = totallen - N;
            List<byte[]> splitbytes = new List<byte[]>();
            var blocksize = srcdata.Length / totallen;
            if (srcdata.Length % totallen != 0)
                blocksize++;

            SplitBytes(srcdata, splitbytes, blocksize);
            ECElement[] eles = new ECElement[totallen];
            for (var i = 0; i < totallen; i++)
            {
                eles[i] = new ECElement();
                eles[i].index = (byte)i;
                eles[i].block = splitbytes[i];
                eles[i].byteecc = new List<byte[]>();
                for (var j = 0; j < blocksize; j++)
                {
                    eles[i].byteecc.Add(null);
                    //break;
                }
            }

            var data = new byte[totallen];
            for (var x = 0; x < eles[0].block.Length; x++)
            {
                //fill data;
                for (var i = 0; i < totallen; i++)
                {
                    data[i] = eles[i].block[x];
                }
                var eccdata = ReedSolomon.ReedSolomonAlgorithm.Encode(data, checkecc);

                for (var i = 0; i < totallen; i++)
                {
                    //if (eles[i].byteecc[0] == null)
                    eles[i].byteecc[x] = eccdata;
                }
            }
            return eles;
        }
        public static byte[] JoinDataWithBFTN(ECElement[] eles, int N = 1)
        {
            var totallen = 3 * N + 1;
            var checkecc = totallen - N;
            var data = new byte[totallen];
            var dataecc = new byte[checkecc];
            int blocksize = eles[0].block.Length;
            var finaldata = new byte[totallen * blocksize];
            for (var x = 0; x < eles[0].block.Length; x++)
            {
                foreach (var e in eles)
                {
                    data[e.index] = e.block[x];
                    dataecc = e.byteecc[x];
                }
                var srcdata = ReedSolomon.ReedSolomonAlgorithm.Decode(data, dataecc);
                for (var i = 0; i < srcdata.Length; i++)
                {
                    int pos = i * blocksize + x;
                    finaldata[pos] = srcdata[i];
                }
            }
            return finaldata;
        }
    }
}
