namespace SharpRoyale_Asset_Downloader.Core.Compression
{
    using SevenZip.SDK.Compress.LZMA;
    using System;
    using System.IO;

    internal class LZMA
    {
        internal static Decoder _Decoder = new Decoder();

        internal static void Decompress(string _SourcePath, string _OutputPath, File_Type _Type)
        {
            using (FileStream _InputStream = new FileStream(_SourcePath, FileMode.Open))
            {
                using (FileStream _OutputStream = new FileStream(_OutputPath, FileMode.Create))
                {
                    switch (_Type)
                    {
                        case File_Type.CSV:
                            {
                                byte[] _Properties = new byte[5];
                                _InputStream.Read(_Properties, 0, 5);

                                byte[] _Buffer = new byte[4];
                                _InputStream.Read(_Buffer, 0, 4);

                                _Decoder.SetDecoderProperties(_Properties);
                                _Decoder.Code(_InputStream, _OutputStream, _InputStream.Length, BitConverter.ToInt32(_Buffer, 0), null);

                                break;
                            }

                        case File_Type.SC:
                            {
                                _InputStream.Seek(26, SeekOrigin.Current);

                                byte[] _Properties = new byte[5];
                                _InputStream.Read(_Properties, 0, 5);

                                byte[] _Buffer = new byte[4];
                                _InputStream.Read(_Buffer, 0, 4);

                                _Decoder.SetDecoderProperties(_Properties);
                                _Decoder.Code(_InputStream, _OutputStream, _InputStream.Length, BitConverter.ToInt32(_Buffer, 0), null);

                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }
    }
}
