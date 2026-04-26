using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

public class MakeModInfo : Microsoft.Build.Utilities.Task
{
    [Required] public string Authors { get; set; } = null!;

    public string ModType { get; set; } = "code";

    [Required] public string Name { get; set; } = null!;
    [Required] public string Version { get; set; } = null!;
    public string? Description { get; set; }

    [Required] public ITaskItem[] Dependencies { get; set; } = [];

    [Required] public string FilePath { get; set; } = "";

    public string? Side { get; set; } = "universal";

    public bool RequiredOnClient { get; set; } = true;
    public bool RequiredOnServer { get; set; } = true;
    public string? Website { get; set; }
    public string? IconPath { get; set; }


    public string? Contributors { get; set; }

    public int TextureSize { get; set; }
    [Required] public string ModId { get; set; } = null!;
    public string? NetworkVersion { get; set; }


    public async Task<bool> ExecuteAsync()
    {
        switch (Side?.ToLower())
        {
            case "universal":
            case "client":
            case "server":
                break;
            default:
                throw new InvalidEnumArgumentException($"Side {Side} is not a valid value for AppSide");
        }

        switch (ModType?.ToLower())
        {
            case "code":
            case "content":
            case "theme":
                break;
            default:
                throw new InvalidEnumArgumentException($"Side {Side} is not a valid value for AppSide");
        }

        var modInfo = new ModInfo.ModInfo
        {
            Authors = Authors.Split(',').ToList(),
            ModType = ModType?.ToLower() ?? "code",
            Name = Name,
            Version = Version,
            Description = Description,
            Side = Side?.ToLower() ?? "universal",
            RequiredOnClient = RequiredOnClient,
            RequiredOnServer = RequiredOnServer,
            Website = Website ?? "",
            IconPath = IconPath,
            Contributors = Contributors?.Split(',').ToList() ?? [],
            TextureSize = TextureSize,
            ModID = ModId,
            NetworkVersion = NetworkVersion,
            Dependencies = Dependencies.Select(item => new Dependency(item)).ToList(),
        };
        var jsonText = JsonConvert.SerializeObject(modInfo, Formatting.Indented, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new IgnoreEmptyEnumerableResolver(),
        });
        //     RequiredOnClient, RequiredOnServer, new List<ModDependency>());
        // modInfo.IconPath = IconPath;
        // if (TextureSize is not null)
        //     modInfo.TextureSize = TextureSize ?? modInfo.TextureSize;
        // modInfo.NetworkVersion = NetworkVersion;
        //
        // var jsonText = JsonConvert.SerializeObject(modInfo);
        //
#if NETSTANDARD2_0_OR_GREATER
        System.IO.File.WriteAllText(FilePath, jsonText);
#else
        await System.IO.File.WriteAllTextAsync(FilePath, jsonText);
#endif

        Log.LogMessage(MessageImportance.High, $"Made mod info at {FilePath}");

        return !Log.HasLoggedErrors;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}