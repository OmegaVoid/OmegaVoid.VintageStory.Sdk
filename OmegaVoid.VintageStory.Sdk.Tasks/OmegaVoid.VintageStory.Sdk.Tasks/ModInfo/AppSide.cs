using System;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

/// <summary>
/// A server/client side used by for the Vintage Story app.
/// </summary>
[Flags]
public enum AppSide
{
    /// <summary>For server side things only.</summary>
    Server = 1,
    /// <summary>For client side things only.</summary>
    Client = 2,
    /// <summary>For server and client side things.</summary>
    Universal = Client | Server, // 0x00000003
}