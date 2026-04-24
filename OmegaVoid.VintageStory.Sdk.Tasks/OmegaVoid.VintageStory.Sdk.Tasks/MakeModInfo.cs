using System.Threading.Tasks;
using Vintagestory.API.Common;


namespace OmegaVoid.VintageStory.Sdk.Tasks;

public class MakeModInfo : Microsoft.Build.Utilities.Task
{
    public async Task<bool> ExecuteAsync()
    {
        ModInfo
        return true;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}