using LibBundle3.Nodes;
using LibBundle3.Records;

namespace Xiletrade.Json;

internal class FileLib(FileRecord file, string datName) : IFileNode
{
    public FileRecord Record => file;

    public IDirectoryNode? Parent => throw new NotImplementedException();

    public string Name => datName;
}
