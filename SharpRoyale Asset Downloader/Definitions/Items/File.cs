namespace SharpRoyale_Asset_Downloader.Definitions.Items
{
    using Newtonsoft.Json;

    internal class File
    {
        [JsonProperty("file")]
        internal string Name;

        [JsonProperty("sha")]
        internal string Hash;
    }
}
