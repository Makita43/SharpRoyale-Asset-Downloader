namespace SharpRoyale_Asset_Downloader
{
    using Core;
    using Definitions;
    using Helpers;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;

    internal class Hash_Module
    {
        internal Fingerprint Fingerprint;

        internal Socket _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        internal List<byte> _List = new List<byte>();

        internal Hash_Module(string Host)
        {
            _Socket.Connect(Host, 9339);

            _Socket.Send(Session_Request);

            ReceiveSync();
        }

        internal byte[] Session_Request
        {
            get
            {
                List<byte> _Packet = new List<byte>();

                _Packet.AddUShort(10100); // ID
                _Packet.Add(0);
                _Packet.AddUShort(72); // Length
                _Packet.AddUShort(1); // Version

                _Packet.AddInt(1); // Protocol
                _Packet.AddInt(Settings.Key_Version); // Key Version
                _Packet.AddInt(Settings.Major_Version); // Major Version
                _Packet.AddInt(0); // Revision
                _Packet.AddInt(Settings.Minor_Version); // Minor Version
                _Packet.AddString("9e5bf715eec4b3a9706515c21f7b519713b2d906"); // Hash from Clash of Clans 5.2.4 :)
                _Packet.AddInt(2);
                _Packet.AddInt(2); // Store

                return _Packet.ToArray();
            }
        }

        internal void ReceiveSync()
        {
            byte[] _Data = new byte[2048];

            ushort Length = 0, Version = 0;

            while (true)
            {
                try
                {
                    int _ReceivedBytes = _Socket.Receive(_Data);

                    if (_ReceivedBytes >= 7)
                    {
                        byte[] _Buffer = new byte[_ReceivedBytes];

                        Array.Copy(_Data, _Buffer, _ReceivedBytes);

                        using (Reader _Reader = new Reader(_Buffer))
                        {
                            if (Length == 0 && _Reader.ReadUInt16() == 20103)
                            {                           
                                _Reader.Seek(1, System.IO.SeekOrigin.Current);

                                Length  = _Reader.ReadUInt16();
                                Version = _Reader.ReadUInt16();

                                _List.Clear();

                                _List.AddRange(_Reader.ReadAllBytes);
                            }
                            else
                            {
                                _List.AddRange(_Buffer);

                                if (_Socket.Available == 0 && Length <= (_List.Count / 7) - 610)
                                {
                                    _Socket.Shutdown(SocketShutdown.Both);
                                    _Socket.Close();

                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            using (Reader _Reader = new Reader(_List.ToArray()))
            {
                _Reader.ReadByte(); // Error Code

                Fingerprint = JsonConvert.DeserializeObject<Fingerprint>(_Reader.ReadString());
            }
        }
    }
}
