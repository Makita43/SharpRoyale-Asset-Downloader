namespace SharpRoyale_Asset_Downloader
{
    using Core;
    using Definitions;
    using Newtonsoft.Json;
    using SevenZip.SDK.Compress.LZMA;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class Program
    {
        internal static Hash_Module Hash_Module;

        internal static Prefixed _Prefixed = new Prefixed();
        internal static Decoder _Decoder   = new Decoder();

        public static void Main(string[] args)
        {
            IntPtr Handle = GetConsoleWindow();
            ShowWindow(Handle, 3);
            SetWindowLong(Handle, -20, (int)GetWindowLong(Handle, -20) ^ 0x80000);
            SetLayeredWindowAttributes(Handle, 0, 227, 0x2);

            Console.Title = "[SHR] | SharpRoyale Asset Downloader | 1.0.0";

            Console.WriteLine(@"
       _________.__                       __________                     .__          
      /   _____/|  |__ _____ _____________\______   \ ____ ___.__._____  |  |   ____  
      \_____  \ |  |  \\__  \\_  __ \____ \|       _//  _ <   |  |\__  \ |  | _/ __ \ 
      /        \|   Y  \/ __ \|  | \/  |_> >    |   (  <_> )___  | / __ \|  |_\  ___/ 
     /_______  /|___|  (____  /__|  |   __/|____|_  /\____// ____|(____  /____/\___  >
             \/      \/     \/      |__|          \/       \/          \/          \/ Asset Downloader");

            Console.SetOut(_Prefixed);
            Console.WriteLine();

            Console.WriteLine("Starting...\n");

            Hash_Module = new Hash_Module("game.clashroyaleapp.com");

            if (!ReferenceEquals(null, Hash_Module.Fingerprint))
            {
                Console.WriteLine($"Downloaded Fingerprint, Hash: {Hash_Module.Fingerprint.Hash} - Version: {Hash_Module.Fingerprint.Version}.\n");

                if (!File.Exists("fingerprint.json"))
                {
                    File.WriteAllText("fingerprint.json", JsonConvert.SerializeObject(Hash_Module.Fingerprint));

                    Console.WriteLine($"Fingerprint has been saved.");
                }
                else
                {
                    Fingerprint _Fingerprint = JsonConvert.DeserializeObject<Fingerprint>(File.ReadAllText("fingerprint.json"), new JsonSerializerSettings() { Formatting = Formatting.Indented });

                    if (_Fingerprint.Hash == Hash_Module.Fingerprint.Hash)
                    {
                        Console.WriteLine($"No new Patch found.");
                        Console.ReadKey(true);
                        Environment.Exit(0);
                    }
                }

                if (!Directory.Exists("Output"))
                {
                    Directory.CreateDirectory("Output");

                    Console.WriteLine("Output directory has been created.\n");
                }
                else
                {
                    Directory.Delete("Output", true);
                    Directory.CreateDirectory("Output");

                    Console.WriteLine("Output directory has been cleared.\n");
                }

                Console.WriteLine($"Downloading {Hash_Module.Fingerprint.Files.Count} Files...\n\n");

                using (WebClient _Client = new WebClient())
                {
                    int Downloaded = 0;

                    foreach (var _File in Hash_Module.Fingerprint.Files)
                    {
                        string URL = $"{Settings.Hosts[0]}{Hash_Module.Fingerprint.Hash}/{_File.Name}";

                        try
                        {
                            if (!Directory.Exists($"Output/compressed/{Path.GetDirectoryName(_File.Name)}"))
                            {
                                Directory.CreateDirectory($"Output/compressed/{Path.GetDirectoryName(_File.Name)}");
                            }

                            _Client.DownloadFile(URL, $"Output/compressed/{_File.Name}");

                            if (_File.Name.EndsWith(".csv") || _File.Name.EndsWith(".sc"))
                            {
                                if (!Directory.Exists($"Output/decompressed/{Path.GetDirectoryName(_File.Name)}"))
                                {
                                    Directory.CreateDirectory($"Output/decompressed/{Path.GetDirectoryName(_File.Name)}");
                                }

                                using (FileStream _FS = new FileStream($"Output/compressed/{_File.Name}", FileMode.Open))
                                {
                                    using (FileStream _Stream = new FileStream($"Output/decompressed/{_File.Name}", FileMode.Create))
                                    {
                                        if (_File.Name.EndsWith(".sc"))
                                        {
                                            _FS.Seek(26, SeekOrigin.Current);                                  

                                            byte[] _Properties = new byte[5];
                                            _FS.Read(_Properties, 0, 5);

                                            byte[] _Buffer = new byte[4];
                                            _FS.Read(_Buffer, 0, 4);

                                            int _OutLength = BitConverter.ToInt32(_Buffer, 0);

                                            _Decoder.SetDecoderProperties(_Properties);
                                            _Decoder.Code(_FS, _Stream, _FS.Length, _OutLength, null);
                                        }
                                        else
                                        {
                                            byte[] _Properties = new byte[5];
                                            _FS.Read(_Properties, 0, 5);

                                            byte[] _Buffer = new byte[4];
                                            _FS.Read(_Buffer, 0, 4);

                                            int _OutLength = BitConverter.ToInt32(_Buffer, 0);

                                            _Decoder.SetDecoderProperties(_Properties);
                                            _Decoder.Code(_FS, _Stream, _FS.Length, _OutLength, null);
                                        }                                     
                                    }
                                }
                            }

                            Downloaded++;

                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"Progress: {Math.Round((double)(100 * Downloaded) / Hash_Module.Fingerprint.Files.Count)}%, {Downloaded}/{Hash_Module.Fingerprint.Files.Count}");
                        }
                        catch (Exception)
                        {
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Downloaded {Downloaded}/{Hash_Module.Fingerprint.Files.Count} Files.");
                }
            }
            else
            {
                Console.WriteLine("Failed to download the Fingerprint.");
            }

            Console.ReadKey(true);        
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
    }
}
