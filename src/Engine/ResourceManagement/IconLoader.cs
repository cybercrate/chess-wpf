using System.Windows.Media.Imaging;

namespace Engine.ResourceManagement;

public static class IconLoader
{
    private const string RelativePath = $"{ResourcePath.ResourcesRelativePath}/Icons";
    
    private const string Main = "MainIcon.ico";

    private const string Back = "BackIcon.png";
    private const string Exit = "ExitIcon.png";
    private const string Forward = "ForwardIcon.png";
    private const string New = "NewIcon.png";
    private const string Open = "OpenIcon.png";
    private const string Save = "SaveIcon.png";
    private const string Settings = "SettingsIcon.png";
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static BitmapImage GetImage(IconType type) => new(new Uri(GeneratePath(type), UriKind.Relative));

    private static string GeneratePath(IconType type)
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
