namespace SharpRoyale_Asset_Downloader
{
    using Core;
    using Core.Compression;
    using Core.SC;
    using Definitions;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class Program
    {
        internal static Hash_Module Hash_Module;

        internal static Prefixed _Prefixed = new Prefixed();

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

            Console.WriteLine("Menu:");
            Console.WriteLine(" -> Press A to check for a new patch.");
            Console.WriteLine(" -> Press B to convert all SC Textures to PNG. [EXPERIMENTAL]");
            Console.WriteLine(" -> Press C to delete the Fingerprint.\n");

            _Entry:
            ConsoleKeyInfo _Input = Console.ReadKey(true);

            switch (_Input.Key)
            {
                case ConsoleKey.A:
                    {
                        Console.WriteLine("Downloading Fingerprint...");

                        Hash_Module = new Hash_Module("game.clashroyaleapp.com");

                        if (!ReferenceEquals(null, Hash_Module.Fingerprint))
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"Downloaded Fingerprint, Hash: {Hash_Module.Fingerprint.Hash} - Version: {Hash_Module.Fingerprint.Version}.\n");

                            if (!File.Exists("fingerprint.json"))
                            {
                                File.WriteAllText("fingerprint.json", JsonConvert.SerializeObject(Hash_Module.Fingerprint, new JsonSerializerSettings() { Formatting = Formatting.Indented }));

                                Console.WriteLine($"Fingerprint has been saved.");
                            }
                            else
                            {
                                Fingerprint _Fingerprint = JsonConvert.DeserializeObject<Fingerprint>(File.ReadAllText("fingerprint.json"));

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

                                            if (_File.Name.EndsWith(".sc"))
                                            {
                                                LZMA.Decompress($"Output/compressed/{_File.Name}", $"Output/decompressed/{_File.Name}", File_Type.SC);
                                            }
                                            else
                                            {
                                                LZMA.Decompress($"Output/compressed/{_File.Name}", $"Output/decompressed/{_File.Name}", File_Type.CSV);
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
                                Console.WriteLine($"Downloaded {Downloaded} of {Hash_Module.Fingerprint.Files.Count} Files.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to download the Fingerprint.");
                        }

                        break;
                    }

                case ConsoleKey.B:
                    {
                        if (!Directory.Exists("Output/decompressed/sc"))
                        {
                            Console.WriteLine("SC Directory not found. Please re-download the Assets.");
                            break;
                        }

                        if (!Directory.Exists("Output/png"))
                        {
                            Directory.CreateDirectory("Output/png");
                        }

                        int Total = 0, Converted = 0;

                        foreach (string _File in Directory.GetFiles("Output/decompressed/sc"))
                        {
                            if (_File.EndsWith("_tex.sc"))
                            {
                                Total++;
                            }
                        }

                        Console.WriteLine();

                        foreach (string _File in Directory.GetFiles("Output/decompressed/sc"))
                        {
                            if (_File.EndsWith("_tex.sc"))
                            {
                                SC2PNG.ExportTextures(_File, $"Output/png/{Path.GetFileNameWithoutExtension(_File)}.png");
                                Converted++;

                                if (Converted > 1)
                                {
                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                    Console.WriteLine($"Progress: {Math.Round((double)(100 * Converted) / Total)}%, {Converted}/{Total}");
                                }
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine($"Converted {Converted} of {Total} Textures.");

                        break;
                    }

                case ConsoleKey.C:
                    {
                        if (File.Exists("fingerprint.json"))
                        {
                            File.Delete("fingerprint.json");

                            Console.WriteLine("Fingerprint has been deleted.");
                        }
                        else
                        {
                            Console.WriteLine("Fingerprint not found. Please re-download the Assets.");
                        }                

                        break;
                    }

                default:
                    {
                        goto _Entry;
                    }
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
