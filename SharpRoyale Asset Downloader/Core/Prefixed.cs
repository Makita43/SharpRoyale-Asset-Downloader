namespace SharpRoyale_Asset_Downloader.Core
{ 
    using System;
    using System.IO;
    using System.Text;

    internal class Prefixed : TextWriter
    {
        public readonly TextWriter _Original = null;

        public Prefixed()
        {
            _Original = Console.Out;
        }

        public override Encoding Encoding
        {
            get
            {
                return new ASCIIEncoding();
            }
        }

        public override void Write(string _Text)
        {
            _Original.Write(_Text);
        }

        public override void WriteLine(string _Text)
        {
            _Original.WriteLine($"[SHR]    {_Text}");
        }

        public override void WriteLine()
        {
            _Original.WriteLine();
        }
    }
}
