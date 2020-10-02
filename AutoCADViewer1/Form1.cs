using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using netDxf;

namespace AutoCADViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";//默认路径，注意这里写路径时要用c:\\而不是c:\
            openFileDialog1.Filter = "Dwg文件|*.dwg";//过滤的文件，以|隔开，如“文本文件|*.*|Java文件|*.java”
            openFileDialog1.RestoreDirectory = true;//但打开对话框后，文件内容有改变了，是否同步刷新
            openFileDialog1.FilterIndex = 1;//当filter有多个时，选择默认的filter，注意，下标时从1开始，如果只有一个filter可以不用写这个属性
            if (openFileDialog1.ShowDialog() == DialogResult.OK)//这个是关键，意思是当你选择了文件后并点击了OK按钮
            {
                
           
                pictureBox1.Image= ViewDWG.GetDwgImage(openFileDialog1.FileName);

            }
        }

        
    }
    class ViewDWG
    {
        public struct BITMAPFILEHEADER
        {
            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }
        public static Image GetDwgImage(string FileName)
        {
            if (!(File.Exists(FileName)))
            {
                throw new FileNotFoundException("文件没有被找到");
            }
            FileStream DwgF;  //文件流
            int PosSentinel;  //文件描述块的位置
            BinaryReader br;  //读取二进制文件
            int TypePreview;  //缩略图格式
            int PosBMP;       //缩略图位置
            int LenBMP;       //缩略图大小
            short biBitCount; //缩略图比特深度
            BITMAPFILEHEADER biH; //BMP文件头，DWG文件中不包含位图文件头，要自行加上去
            byte[] BMPInfo;       //包含在DWG文件中的BMP文件体
            MemoryStream BMPF = new MemoryStream(); //保存位图的内存文件流
            BinaryWriter bmpr = new BinaryWriter(BMPF); //写二进制文件类
            Image myImg = null;
            try
            {
                DwgF = new FileStream(FileName, FileMode.Open, FileAccess.Read);   //文件流
                br = new BinaryReader(DwgF);
                DwgF.Seek(13, SeekOrigin.Begin); //从第十三字节开始读取
                PosSentinel = br.ReadInt32();  //第13到17字节指示缩略图描述块的位置
                DwgF.Seek(PosSentinel + 30, SeekOrigin.Begin);  //将指针移到缩略图描述块的第31字节
                TypePreview = br.ReadByte();  //第31字节为缩略图格式信息，2 为BMP格式，3为WMF格式
                if (TypePreview == 1)
                {
                }
                else if (TypePreview == 2 || TypePreview == 3)
                {
                    PosBMP = br.ReadInt32(); //DWG文件保存的位图所在位置
                    LenBMP = br.ReadInt32(); //位图的大小
                    DwgF.Seek(PosBMP + 14, SeekOrigin.Begin); //移动指针到位图块
                    biBitCount = br.ReadInt16(); //读取比特深度
                    DwgF.Seek(PosBMP, SeekOrigin.Begin); //从位图块开始处读取全部位图内容备用
                    BMPInfo = br.ReadBytes(LenBMP); //不包含文件头的位图信息
                    br.Close();
                    DwgF.Close();
                    biH.bfType = 19778; //建立位图文件头
                    if (biBitCount < 9)
                    {
                        biH.bfSize = 54 + 4 * (int)(Math.Pow(2, biBitCount)) + LenBMP;
                    }
                    else
                    {
                        biH.bfSize = 54 + LenBMP;
                    }
                    biH.bfReserved1 = 0; //保留字节
                    biH.bfReserved2 = 0; //保留字节
                    biH.bfOffBits = 14 + 40 + 1024; //图像数据偏移
                    //以下开始写入位图文件头
                    bmpr.Write(biH.bfType); //文件类型
                    bmpr.Write(biH.bfSize);  //文件大小
                    bmpr.Write(biH.bfReserved1); //0
                    bmpr.Write(biH.bfReserved2); //0
                    bmpr.Write(biH.bfOffBits); //图像数据偏移
                    bmpr.Write(BMPInfo); //写入位图
                    BMPF.Seek(0, SeekOrigin.Begin); //指针移到文件开始处
                    myImg = Image.FromStream(BMPF); //创建位图文件对象
                    bmpr.Close();
                    BMPF.Close();
                }
                return myImg;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }


}
