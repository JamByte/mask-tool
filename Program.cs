using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
namespace Monkeywrenchcsharp
{
    class Program
    {
        public static string masklocation;
        public static string outputfilelocation;
        public static int height;
        public static int width;
        public static int size;
        public static byte[] maskbytes;
        public static byte[] tempbyte;
        public static int[,] pixeldata;
        public static int[] flagpair = new int[2];
        public static int flags;
        public static byte[,] colours = { { 0, 255, 0 }, { 128, 128, 128 },{ 0, 128, 0 },{ 0, 0, 0 },{ 0, 255, 255 },{ 0, 0, 255 },{ 0, 128, 128 },{ 0, 0, 128 } };
        public static Bitmap image;
        public static int h;
        public static int w;
        public static byte[][] datatemp;
        public static int[] colourss = new int[] { -1, -1 };
        public static byteholder[] colourbytes;
        public static int pos=0;
        public static  List<byte> maskbytelist;
        public static int lastpos=0;
        public static int lastc = -1;
        public static int r12;
        static void Main(string[] args)
        {
            if (args[0] == "decode")
            {
                masklocation = args[1];
                outputfilelocation = args[2];
                maskbytes = File.ReadAllBytes(masklocation);
                tempbyte = new byte[2];
                tempbyte[0] = maskbytes[0];
                tempbyte[1] = maskbytes[1];
                width = BitConverter.ToUInt16(tempbyte, 0);
                tempbyte[0] = maskbytes[4];
                tempbyte[1] = maskbytes[5];
                height = BitConverter.ToUInt16(tempbyte, 0);
                size = width * height;
                image = new Bitmap(width, height);
                pixeldata = new int[size, 3];
                Console.WriteLine(width);
                Console.WriteLine(height);
                Console.WriteLine(maskbytes.Length);
                datatemp = new byte[size][];
                for (int i = 0; i < datatemp.Length - 1; i++)
                {
                    datatemp[i] = new byte[] { 0, 0, 0 };
                }

                for (int i = 0; i < size && pos < size; i += 2)
                {
                    w++;
                    tempbyte[0] = maskbytes[i + 8];
                    tempbyte[1] = maskbytes[i + 9];
                    flagpair[0] = (tempbyte[1] & 15);
                    flagpair[1] = (tempbyte[1] >> 4);
                    for (int j = 0; j < tempbyte[0] * 2; j++)
                    {
                        flags = flagpair[j % 2];
                        flags &= 7;
                        //image.SetPixel(w, h, Color.FromArgb(255, colours[flags & 7, 0], colours[flags & 7, 1], colours[flags & 7, 2]));
                        try
                        {
                            datatemp[pos + j][0] = colours[flags & 7, 0];
                            datatemp[pos + j][1] = colours[flags & 7, 1];
                            datatemp[pos + j][2] = colours[flags & 7, 2];
                        }
                        catch { }

                    }
                    pos += tempbyte[0] * 2;
                    if (w == width - 1)
                    {
                        h++;
                        w = 0;
                    }


                }
                CreateBitmap24bppRgb(datatemp, width, height).Save(outputfilelocation);
            }
            else
            {
                image = new Bitmap(args[1]);
                maskbytelist = new List<byte>();
                maskbytelist.Add(BitConverter.GetBytes((short)image.Width)[0]);
                maskbytelist.Add( BitConverter.GetBytes((short)image.Width)[1]);
                maskbytelist.Add(0);
                maskbytelist.Add(0);
                maskbytelist.Add( BitConverter.GetBytes((short)image.Height)[0]);
                maskbytelist.Add(BitConverter.GetBytes((short)image.Height)[1]);
                maskbytelist.Add(0);
                maskbytelist.Add(0);
                colourbytes = new byteholder[image.Width * image.Height];
                for (int i = 0; i < colourbytes.Length; i++)
                {
                    colourbytes[i] = new byteholder();
                }
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int b = 0; b < 8; b++)
                        {
                            if(colours[b,0] == image.GetPixel(x,y).R && colours[b, 1] == image.GetPixel(x, y).G&& colours[b, 2] == image.GetPixel(x, y).B)
                            {
                                colourbytes[pos].pos = pos;
                                colourbytes[pos].bytevalue = (byte)b;
                                break;
                            }
                        }
                        pos++;
                    }
                    //    pos += offset;  
                }
                Console.WriteLine((int)Math.Floor((double)5 / 2));
                for (int i = 0; i < colourbytes.Length; i++)
                {

                  
                    if (colourbytes[i].bytevalue >0)
                    {
                        colourbytes[i].bytevalue |= 8;
                    }
                    r12 = colourbytes[i].pos - lastpos;
                    if (r12 < 2)
                    {
                        colourss[r12] = colourbytes[i].bytevalue;
                    }
                    else
                    {
                        if (colourbytes[i].bytevalue != colourss[r12 % 2])
                        {
                            write_run(maskbytelist, (int)Math.Floor((double)r12 / 2), (byte)(colourss[0] | colourss[1] << 4));
                            lastpos += 2 * (int)Math.Floor((double)r12 / 2);
                            if ((r12 % 2) == 0){
                                colourss[0] = colourbytes[i].bytevalue; }
                            else
                            {
                                colourss[0] = lastc;
                                colourss[1] = colourbytes[i].bytevalue;
                            }

                        }
                    }
                    lastc = colourbytes[i].bytevalue;
                }
                r12 = (image.Width * image.Height) - lastpos;
                write_run(maskbytelist, (int)Math.Floor((double)r12 / 2), (byte)(colourss[0] | colourss[1] << 4));
                var bytearray = new List<byte>();
                
                File.WriteAllBytes(args[2], maskbytelist.ToArray());
            }
        }
        public static  List<byte> write_run(List<byte> bytes, int r1, byte data)
        {
            while (r1 > 0)
            {
                if (r1 <= 255)
                {
                    bytes.Add((byte)r1);
                    bytes.Add(data);
                    
                }
                else
                {
                    bytes.Add(255);
                    bytes.Add(data);
                }
                r1 -= 255;
            

            }
            return bytes;
        }
        public static Bitmap CreateBitmap24bppRgb(byte[][] bmpData, int width, int height)
        {
            if ((width * height ) != bmpData.Length)
                throw new ArgumentException();

            // Sometimes rows have an offset to complete a multiple of 4 number of bytes.  
            // In that case:  
            // int offset = 4 - ((width * 3) % 4);  
            // if (offset == 4) offset = 0;  
            // if (((width * 3 + offset) * height) != bmpData.Length)  
            //     throw new ArgumentException();  

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            int pos = 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        bmp.SetPixel(x, y, Color.FromArgb(bmpData[pos][0], bmpData[pos][1], bmpData[pos][2]));
                    }
                    catch { }
                    pos += 1;
                }
                //    pos += offset;  
            }
            return bmp;
        }
    }
    public class byteholder
    {
        public byte bytevalue=0;
        public int pos=0;
        public byteholder() { }
    }
}
