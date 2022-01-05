namespace Engine.ResourceManagement;

public static class IconLoader
{
    private const string RelativePath = $"{ResourcePath.ResourcesRelativePath}/Icons";

    private const string Back = "BackIcon.png";
    private const string Exit = "ExitIcon.png";
    private const string Forward = "FowardIcon.png";
    private const string Main = "BackIcon.ico";
    private const string New = "BackIcon.png";
    private const string Open = "BackIcon.png";
    private const string Save = "BackIcon.png";
    private const string Settings = "BackIcon.png";
    
    public static string GeneratePath(IconType type)
    {
        var imageFile = type switch
        {
            IconType.Back => Back,
            IconType.Exit => Exit,
            IconType.Forward => Forward,
            IconType.Main => Main,
            IconType.New => New,
            IconType.Open => Open,
            IconType.Save => Save,
            IconType.Settings => Settings,
            _ => throw new Exception()
        };

        return $"{RelativePath}/{imageFile}";
    }
}
