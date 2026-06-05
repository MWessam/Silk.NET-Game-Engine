namespace LunarEngine.Assets;

public class FileSystemAssetProvider : IAssetProvider
{
    private readonly string _rootDirectory;

    public FileSystemAssetProvider(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public string ResolvePath(AssetKey key) => Path.Combine(_rootDirectory, key.Name);

    public Stream OpenStream(AssetKey key) => File.OpenRead(ResolvePath(key));

    public bool Exists(AssetKey key) => File.Exists(ResolvePath(key));
}
