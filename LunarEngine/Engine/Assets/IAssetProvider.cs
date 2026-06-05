namespace LunarEngine.Assets;

public readonly struct AssetKey(string category, string name)
{
    public string Category { get; } = category;
    public string Name { get; } = name;

    public override string ToString() => $"{Category}:{Name}";
}

public interface IAssetProvider
{
    string ResolvePath(AssetKey key);
    Stream OpenStream(AssetKey key);
    bool Exists(AssetKey key);
}
