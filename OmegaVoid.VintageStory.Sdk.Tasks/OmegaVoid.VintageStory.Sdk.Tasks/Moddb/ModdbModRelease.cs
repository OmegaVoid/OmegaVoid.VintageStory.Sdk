using Newtonsoft.Json;


namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModRelease
{
    [JsonProperty("releaseid")] public int ReleaseId { get; set; }
    [JsonProperty("fileid")] public int FileId { get; set; }
    [JsonProperty("mainfile")] public string MainFile { get; set; }
    [JsonProperty("filename")] public string FileName { get; set; }
    [JsonProperty("modidstr")] public string IdString { get; set; }
    [JsonProperty("modversion")] public string Version { get; set; }
}