using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test001
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int primitive = 5;
            int size = 55;
            int genBase = 2;
            var gf = new ReedSolomon.GenericGF(primitive, size, genBase);
            gf.Initialize();
            var encoder = new ReedSolomon.ReedSolomonEncoder(gf);

            //5 bytes 3ecc ,can break 1 src   4/5
            //5 bytes 4ecc ,can break 2 src   3/5
            //4 bytes 3ecc ,can break 1 src   3/4
            //4 bytes 2ecc ,can break 1 src   3/4
            //4 bytes 1ecc ,can break 1 src   3/4
            //7 bytes 3ecc             1      6/7
            //7 bytes 4ecc                    5/7
            //7 bytes 5ecc                    5/7
            //7 bytes 6ecc can break 3 src    4/7
            //10 bytes 7ecc
            //m =  1 2 3 4 , n =3
            //首先可以容忍的损失 必须小于ecc/2
            // 9dat 6ecc , can break 3
            var srcdata = new byte[] { 221, 2, 3, 5, 33, 0, 11, 00, 11, 22 };
            var eccbyte = 7;
            var eccdata = ReedSolomon.ReedSolomonAlgorithm.Encode(srcdata, eccbyte);

            {
                var srcdat1 = srcdata.Clone() as byte[];
                srcdat1[0] = 10;
                var src = ReedSolomon.ReedSolomonAlgorithm.Decode(srcdat1, eccdata);
                Console.WriteLine("finish it.");
            }
            {
                var srcdat1 = srcdata.Clone() as byte[];
                srcdat1[0] = 0;
                srcdat1[1] = 40;
                var src = ReedSolomon.ReedSolomonAlgorithm.Decode(srcdat1, eccdata);
                Console.WriteLine("finish it.");
            }
            {
                var srcdat1 = srcdata.Clone() as byte[];
                srcdat1[0] = 0;
                srcdat1[1] = 0;
                srcdat1[2] = 0;
                var src = ReedSolomon.ReedSolomonAlgorithm.Decode(srcdat1, eccdata);
                Console.WriteLine("finish it.");
            }
            {
                var srcdat1 = srcdata.Clone() as byte[];
                srcdat1[0] = 0;
                srcdat1[1] = 0;
                srcdat1[2] = 0;
                srcdat1[3] = 0;
                var src = ReedSolomon.ReedSolomonAlgorithm.Decode(srcdat1, eccdata);
                Console.WriteLine("finish it.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {//Split
            var txtbin = System.Text.Encoding.UTF8.GetBytes(this.textBox1.Text);
            var base64str = Convert.ToBase64String(txtbin);
            var srcdata = System.Text.Encoding.UTF8.GetBytes(base64str);
            var N = int.Parse(this.textBox2.Text);
            var eles = ECSplit.SplitDataWithBFTN(srcdata, N);
            this.listBox2.Items.Clear();
            for (var i = 0; i < eles.Length; i++)
            {
                this.listBox2.Items.Add(eles[i]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.listBox2.SelectedItem != null)
            {
                AddElement(this.listBox2.SelectedItem as ECElement);
            }
        }
        void AddElement(ECElement ele)
        {
            foreach (ECElement e in this.listBox3.Items)
            {
                if (e.index == ele.index)
                    return;
            }
            this.listBox3.Items.Add(ele);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                var N = int.Parse(this.textBox3.Text);
                var eles = new ECElement[this.listBox3.Items.Count];
                for (var i = 0; i < this.listBox3.Items.Count; i++)
                {
                    eles[i] = this.listBox3.Items[i] as ECElement;
                }
                var bytes = ECSplit.JoinDataWithBFTN(eles, N);
                var endpos = bytes.Length;
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] == 0)
                    {
                        endpos = i;
                        break;
                    }
                }
                var base64str = System.Text.Encoding.UTF8.GetString(bytes, 0, endpos);
                var srcbyte = Convert.FromBase64String(base64str);
                var info = System.Text.Encoding.UTF8.GetString(srcbyte);
                this.label2.Text = info;
            }
            catch(Exception err)
            {
                MessageBox.Show("join fail:" + err.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                this.textBox5.Text = listBox2.SelectedItem.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                var bytes = new byte[this.textBox4.Text.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    var sub = this.textBox4.Text.Substring(i * 2, 2);
                    bytes[i] = byte.Parse(sub,System.Globalization.NumberStyles.HexNumber);
                }
                var ele = new ECElement();
                ele.FromBytes(bytes);
                AddElement(ele);
            }
            catch
            {

            }
        }
    }
}
