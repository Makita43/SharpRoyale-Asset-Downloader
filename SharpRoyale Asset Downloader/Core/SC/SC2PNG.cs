namespace SharpRoyale_Asset_Downloader.Core.SC
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    internal class SC2PNG
    {
        internal static void ExportTextures(string _SourcePath, string _OutputPath)
        {
            using (BinaryReader _Reader = new BinaryReader(File.OpenRead(_SourcePath)))
            {
                byte ID      = _Reader.ReadByte();
                uint Size    = _Reader.ReadUInt32();
                byte PXFomat = _Reader.ReadByte();

                ushort Width  = _Reader.ReadUInt16(), Heigth = _Reader.ReadUInt16();

                Bitmap _Bitmap = new Bitmap(Width, Heigth, PixelFormat.Format32bppArgb);

                int MTWidth = Width % 32;
                int TTWidth = (Width - MTWidth) / 32;

                int MTHeigth = Heigth % 32;
                int TTHeigth = (Heigth - MTHeigth) / 32;

                Color[,] _PixelArray = new Color[Heigth, Width];

                for (int _Index = 0; _Index < TTHeigth + 1; _Index++)
                {
                    int LHeigth = 32;

                    int XOffset, YOffset;

                    if (_Index == TTHeigth)
                    {
                        LHeigth = MTHeigth;
                    }

                    for (int t = 0; t < TTWidth; t++)
                    {
                        for (int y = 0; y < LHeigth; y++)
                        {
                            for (int x = 0; x < 32; x++)
                            {
                                XOffset = t * 32;
                                YOffset = _Index * 32;

                                _PixelArray[y + YOffset, x + XOffset] = GetColorByPXFormat(_Reader, PXFomat);
                            }
                        }
                    }

                    for (int Y = 0; Y < LHeigth; Y++)
                    {
                        for (int X = 0; X < MTWidth; X++)
                        {
                            int PXOffsetX = TTWidth * 32, PXOffsetY = _Index * 32;

                            _PixelArray[Y + PXOffsetY, X + PXOffsetX] = GetColorByPXFormat(_Reader, PXFomat);
                        }
                    }
                }

                for (int _Row = 0; _Row < _PixelArray.GetLength(0); _Row++)
                {
                    for (int _Column = 0; _Column < _PixelArray.GetLength(1); _Column++)
                    {
                        _Bitmap.SetPixel(_Column, _Row, _PixelArray[_Row, _Column]);
                    }
                }

                _Bitmap.Save(_OutputPath);
            }
        }

        public static Color GetColorByPXFormat(BinaryReader _Reader, int PXFormat)
        {
            Color Color = Color.Red;

            switch (PXFormat)
            {
                case 0:
                    {
                        byte R = _Reader.ReadByte();
                        byte G = _Reader.ReadByte();
                        byte B = _Reader.ReadByte();
                        byte A = _Reader.ReadByte();

                        Color = Color.FromArgb((A << 24) | (R << 16) | (G << 8) | B);

                        break;
                    }

                case 2:
                    {
                        ushort _Color = _Reader.ReadUInt16();

                        int R = ((_Color >> 12) & 0xF) << 4;
                        int G = ((_Color >> 8) & 0xF) << 4;
                        int B = ((_Color >> 4) & 0xF) << 4;
                        int A = (_Color & 0xF) << 4;

                        Color = Color.FromArgb(A, R, G, B);

                        break;
                    }

                case 4:
                    {
                        ushort _Color = _Reader.ReadUInt16();

                        int R = ((_Color >> 11) & 0x1F) << 3;
                        int G = ((_Color >> 5) & 0x3F) << 2;
                        int B = (_Color & 0X1F) << 3;

                        Color = Color.FromArgb(R, G, B);

                        break;
                    }

                default:
                    {
                        Console.WriteLine($"Unknown Pixel Format: {PXFormat}.");
                        break;
                    }
            }

            return Color;
        }
    }
}
