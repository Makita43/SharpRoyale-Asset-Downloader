namespace SharpRoyale_Asset_Downloader.Helpers
{
    using System;
    using System.IO;
    using System.Text;

    public class Reader : BinaryReader
    {
        public Reader(byte[] _Buffer) : base(new MemoryStream(_Buffer))
        {
            
        }

        public byte[] ReadAllBytes
        {
            get
            {
                return ReadBytes((int)BaseStream.Length - (int)BaseStream.Position);
            }
        }

        public override string ReadString()
        {
            int _Length = (int)ReadUInt32();

            if (_Length <= -1 || (_Length > BaseStream.Length - BaseStream.Position))
            {
                return null;
            }

            byte[] _Buffer = ReadBytesWithEndian(_Length, false);
            return Encoding.UTF8.GetString(_Buffer);
        }

        public override ushort ReadUInt16()
        {
            byte[] _Buffer = ReadBytesWithEndian(2);
            return BitConverter.ToUInt16(_Buffer, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] _Buffer = ReadBytesWithEndian(4);
            return BitConverter.ToUInt32(_Buffer, 0);
        }

        public long Seek(long _Offset, SeekOrigin _Origin)
        {
            return BaseStream.Seek(_Offset, _Origin);
        }

        private byte[] ReadBytesWithEndian(int _Count, bool _Endian = true)
        {
            byte[] _Buffer = new byte[_Count];
            BaseStream.Read(_Buffer, 0, _Count);

            if (BitConverter.IsLittleEndian && _Endian)
            {
                Array.Reverse(_Buffer);
            }

            return _Buffer;
        }
    }
}