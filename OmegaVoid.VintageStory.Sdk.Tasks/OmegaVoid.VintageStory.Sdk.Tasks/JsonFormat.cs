using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

[UsedImplicitly]
public class JsonFormat : BuildTask
{
    #region Params

    [UsedImplicitly] public int Indent { get; set; } = 2;
    [RequiredImplicit] public required ITaskItem[] Files { get; set; }
    [RequiredImplicit] public required string OutputPath { get; set; }

    #endregion

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();

    private async Task FormatFile(string fileName, string outputPath)
    {
        var text = await File.ReadAllTextAsync(fileName);
        text = string.Join("\n", text.Split('\n').Where(s => !s.StartsWith("//")));

        // Source - https://stackoverflow.com/a/71129244
        // Posted by huha, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-04-25, License - CC BY-SA 4.0
        var outFileName = fileName.Replace(outputPath, OutputPath).Replace("json5", "json").Replace("yaml", "json");
        var outDir = outFileName.Replace(Path.GetFileName(outFileName), "");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        var config = JObject.Parse(text);
        await using var fs = File.Open(outFileName, FileMode.Create);
        Log.LogMessage(MessageImportance.Low, "Open file {0} from {1} to {2}", outFileName, fileName, outputPath);
        await using var sw = new StreamWriter(fs);
        // ReSharper disable once UseAwaitUsing
        using var jw = new JsonTextWriter(sw);
        jw.Formatting = Formatting.Indented;
        jw.IndentChar = ' ';
        jw.Indentation = Indent;

        var serializer = new JsonSerializer();
        serializer.Serialize(jw, config);
    }

    private async Task<bool> ExecuteAsync()
    {
        try
        {
            foreach (var taskItem in Files)
            {
                var targetPath = OutputPath == null
                    ? taskItem.ItemSpec
                    : Path.Combine(OutputPath,
                        Path.GetRelativePath(taskItem.GetMetadata("BaseDir"), taskItem.ItemSpec));
                // Log.LogMessage(MessageImportance.High,
                // $"Searching Files in {taskItem.ItemSpec}\nOutput Path: {OutputPath}\nTarget Path: {targetPath}");
                if (!Directory.Exists(taskItem.ItemSpec))
                    continue;
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);
                foreach (var file in Directory.EnumerateFiles(taskItem.ItemSpec, "*.json*",
                             SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(taskItem.ItemSpec, "*.yaml",
                             SearchOption.AllDirectories)))
                {
                    Log.LogMessage(MessageImportance.High, $"Processing file {file}");
                    await FormatFile(file, taskItem.GetMetadata("BaseDir"));
                }
            }
        }
        catch (Exception e)
        {
            var files = string.Join("\n\t", Files.Select(item => item.ItemSpec));
            Log.LogError("Task JsonFormat was called with Indent = {0}, OutputPath = {2} on files:\n\t{1}", Indent,
                files, OutputPath);
            Log.LogErrorFromException(e, true, true, "JsonFormat.cs");
        }

        return !Log.HasLoggedErrors;
    }
}