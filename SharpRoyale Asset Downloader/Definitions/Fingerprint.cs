namespace SharpRoyale_Asset_Downloader.Definitions
{
    using Newtonsoft.Json;
    using Definitions.Items;
    using System.Collections.Generic;

    internal class Fingerprint
    {
        [JsonProperty("sha")]
        internal string Hash;

        [JsonProperty("version")]
        internal string Version;

        [JsonProperty("files")]
        internal List<File> Files;
    }
}
