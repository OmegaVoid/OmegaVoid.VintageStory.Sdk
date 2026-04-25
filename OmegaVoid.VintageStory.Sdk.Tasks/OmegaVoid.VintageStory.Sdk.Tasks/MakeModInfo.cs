using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public string ModType { get; set; } = nameof(ModInfo.ModType.Code);

    [Required] public string Name { get; set; } = null!;
    [Required] public string Version { get; set; } = null!;
    public string? Description { get; set; }

    [Required] public ITaskItem[] Dependencies { get; set; } = [];

    [Required]
    public string FilePath { get; set; } = "";

    public string? Side { get; set; } = nameof(AppSide.Universal);

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
        
        if(!Enum.TryParse(Side?[0].ToString().ToUpper()+Side?[1..], out AppSide side))
            throw new InvalidEnumArgumentException($"Side {Side} is not a valid value for {nameof(AppSide)}");
        if(!Enum.TryParse(ModType?[0].ToString().ToUpper()+ModType?[1..], out ModType type))
            throw new InvalidEnumArgumentException($"ModType {ModType} is not a valid value for {nameof(ModInfo.ModType)}");
        var modInfo = new ModInfo.ModInfo
        {
            Authors = Authors.Split(",").ToList(),
            Type = type,
            Name = Name,
            Version = Version,
            Description = Description,
            Side = side,
            RequiredOnClient = RequiredOnClient,
            RequiredOnServer = RequiredOnServer,
            Website = Website ?? "",
            IconPath = IconPath,
            Contributors = Contributors?.Split(",").ToList() ?? [],
            TextureSize = TextureSize,
            ModID = ModId,
            NetworkVersion = NetworkVersion,
            Dependencies = Dependencies.Select(item => new Dependency(item)).ToList(),
        };
        var jsonText = JsonConvert.SerializeObject(modInfo,  Formatting.Indented, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new IgnoreEmptyEnumerableResolver() ,
        });
        //     RequiredOnClient, RequiredOnServer, new List<ModDependency>());
        // modInfo.IconPath = IconPath;
        // if (TextureSize is not null)
        //     modInfo.TextureSize = TextureSize ?? modInfo.TextureSize;
        // modInfo.NetworkVersion = NetworkVersion;
        //
        // var jsonText = JsonConvert.SerializeObject(modInfo);
        //
        await System.IO.File.WriteAllTextAsync(FilePath, jsonText);
        Log.LogMessage(MessageImportance.High, $"Made mod info at {FilePath}");

        return !Log.HasLoggedErrors;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}