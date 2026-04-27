using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

[UsedImplicitly]
public class MakeModInfo : BuildTask
{
    #region Params

    [RequiredImplicit] public string Authors { get; set; }
    [UsedImplicitly] public string ModType { get; set; } = "code";
    [RequiredImplicit] public string Name { get; set; }
    [RequiredImplicit] public string Version { get; set; }
    [UsedImplicitly] public string? Description { get; set; }
    [RequiredImplicit] public ITaskItem[] Dependencies { get; set; }
    [RequiredImplicit] public string FilePath { get; set; }
    [UsedImplicitly] public string? Side { get; set; } = "universal";
    [UsedImplicitly] public bool RequiredOnClient { get; set; } = true;
    [UsedImplicitly] public bool RequiredOnServer { get; set; } = true;
    [UsedImplicitly] public string? Website { get; set; }
    [UsedImplicitly] public string? IconPath { get; set; }
    [UsedImplicitly] public string? Contributors { get; set; }
    [UsedImplicitly] public int TextureSize { get; set; }
    [RequiredImplicit] public string ModId { get; set; }
    [UsedImplicitly] public string? NetworkVersion { get; set; }

    #endregion


    private async Task<bool> ExecuteAsync()
    {
        try
        {
            if (!Enum.TryParse(Side, true, out AppSide side))
                throw new InvalidEnumArgumentException($"Side {Side} is not a valid value for {nameof(AppSide)}");
            if (!Enum.TryParse(ModType, true, out ModType type))
                throw new InvalidEnumArgumentException(
                    $"ModType {ModType} is not a valid value for {nameof(ModInfo.ModType)}");

            var modInfo = new ModInfo.ModInfo
            {
                Authors = Authors.Split(',').ToList(),
                ModType = type,
                Name = Name,
                Version = Version,
                Description = Description,
                Side = side,
                RequiredOnClient = RequiredOnClient,
                RequiredOnServer = RequiredOnServer,
                Website = Website ?? "",
                IconPath = IconPath,
                Contributors = Contributors?.Split(',').ToList() ?? [],
                TextureSize = TextureSize,
                ModID = ModId,
                NetworkVersion = NetworkVersion,
                Dependencies = Dependencies.Select(item => new Dependency(item)).ToList()
            };
            var jsonText = JsonConvert.SerializeObject(modInfo, Formatting.Indented, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new IgnoreEmptyEnumerableResolver()
            });

            await File.WriteAllTextAsync(FilePath, jsonText);

            Log.LogMessage(MessageImportance.High, $"Made mod info at {FilePath}");
        }
        catch (Exception e)
        {
            var dependencies = "\n\t\t" +
                               string.Join("\n\t\t", Dependencies.Select(item => new Dependency(item).ToString()));
            Log.LogError("""
                         Task MakeModInfo was called with parameters:
                             Authors = {1}
                             ModType = {2}
                             Name = {3}
                             Version = {4}
                             Description = {5}
                             FilePath = {6}
                             Side = {7}
                             RequiredOnClient = {8}
                             RequiredOnServer = {9}
                             Website = {10}
                             IconPath = {11}
                             Contributors = {12}
                             TextureSize = {13}
                             ModId = {14}
                             NetworkVersion = {15}
                             Dependencies: {0}
                         """, dependencies, Authors, ModType, Name, Version, Description, FilePath, Side,
                RequiredOnClient, RequiredOnServer, Website, IconPath, Contributors, TextureSize, ModId,
                NetworkVersion);

            Log.LogErrorFromException(e, true, true, "MakeModInfo.cs");
        }

        return !Log.HasLoggedErrors;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}