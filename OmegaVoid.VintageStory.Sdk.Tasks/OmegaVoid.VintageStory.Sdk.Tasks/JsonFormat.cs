using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

public class JsonFormat : Microsoft.Build.Utilities.Task
{
    public int Indent { get; set; } = 2;
    [Required] public ITaskItem[] Files { get; set; } = [];

    public string? OutputPath { get; set; }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();

    public async Task FormatFile(string fileName, string outputPath)
    {
        var text = await File.ReadAllTextAsync(fileName);
        text = string.Join("\n", text.Split("\n").Where(s => !s.StartsWith("//")));

        // Source - https://stackoverflow.com/a/71129244
        // Posted by huha, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-04-25, License - CC BY-SA 4.0

        var config = JObject.Parse(text);
        using var fs = File.Open(Path.Combine(outputPath, Path.GetFileName(fileName.Replace("json5","json"))), FileMode.Create);
        using var sw = new StreamWriter(fs);
        using var jw = new JsonTextWriter(sw);

        jw.Formatting = Formatting.Indented;
        jw.IndentChar = ' ';
        jw.Indentation = Indent;

        var serializer = new JsonSerializer();
        serializer.Serialize(jw, config);
    }

    public async Task<bool> ExecuteAsync()
    {
        foreach (var taskItem in Files)
        {
            var targetPath = OutputPath == null
                ? taskItem.ItemSpec
                : Path.Combine(OutputPath, Path.GetRelativePath(taskItem.GetMetadata("BaseDir"), taskItem.ItemSpec));
            // Log.LogMessage(MessageImportance.High,
                // $"Searching Files in {taskItem.ItemSpec}\nOutput Path: {OutputPath}\nTarget Path: {targetPath}");
            if (!Directory.Exists(taskItem.ItemSpec))
                continue;
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            foreach (var file in Directory.EnumerateFiles(taskItem.ItemSpec, "*.json*", SearchOption.AllDirectories))
            {
                Log.LogMessage(MessageImportance.High, $"Processing file {file}");
                await FormatFile(file, targetPath);
            }
        }

        return !Log.HasLoggedErrors;
    }
}