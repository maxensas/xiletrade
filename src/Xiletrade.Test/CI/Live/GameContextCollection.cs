namespace Xiletrade.Test.CI.Live;

/// <summary>
/// Tests changing the game and the gateway of the urls (static context, also set when the data are loaded) :
/// not run with the other tests.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GameContextCollection
{
    public const string Name = "Game context";
}
