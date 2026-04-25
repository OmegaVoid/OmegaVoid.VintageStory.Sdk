using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using OmegaVoid.VintageStory.Sdk.Tasks.Moddb;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;
using Task = System.Threading.Tasks.Task;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

using System;
using Microsoft.Build.Framework;
using Newtonsoft.Json;

public class Dependencies : Microsoft.Build.Utilities.Task
{
    [Required] public ITaskItem[] Dependency { get; set; } = [];
    [Required] public string OutputDir { get; set; } = "";
    [Output] public ITaskItem[] ModsDownloaded { get; private set; } = [];

    public async Task<bool> ExecuteAsync()
    {
        var items = new List<ITaskItem>();
        if (Directory.Exists(OutputDir))
            Directory.Delete(OutputDir, true);
        Directory.CreateDirectory(OutputDir);
        var dependencies1 = DependencyParser.ParseDependencies(Dependency);
        var dependencies2 = await DependencyParser.FetchModDependencies(dependencies1.Keys);
        
        var matchedDeps = DependencyParser.MatchDependencies(dependencies1, dependencies2);

        foreach (var modRelease in matchedDeps.Select(pair => (ModdbModRelease)pair))
        {
            await modRelease.DownloadDependency(OutputDir);
            var path = Path.Combine(OutputDir, modRelease.FileName);
            Log.LogMessage(MessageImportance.High,$"Downloaded {modRelease} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path)}");
            Log.LogMessage(MessageImportance.High,$"Extracted {modRelease} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path.Replace(".zip",""))}");
            items.Add(new TaskItem(itemSpec: path, new Dictionary<string, string> { { "ModId", modRelease.IdString }, { "Version", modRelease.Version }, { "Zip", $"{OutputDir}/{modRelease.FileName}"}, {"String",
                ((Dependency)modRelease).ToString()} }));
        }

        ModsDownloaded = items.ToArray();

        return !Log.HasLoggedErrors;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}