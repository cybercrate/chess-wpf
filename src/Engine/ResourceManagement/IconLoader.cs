using System.Windows.Media.Imaging;

namespace Engine.ResourceManagement;

public static class IconLoader
{
    private const string RelativePath = $"{ResourcePath.ResourcesRelativePath}/Icons";
    
    private const string Back = "Back.png";
    private const string Exit = "Exit.png";
    private const string Forward = "Forward.png";
    private const string New = "New.png";
    private const string Open = "Open.png";
    private const string Save = "Save.png";
    private const string Settings = "Settings.png";
    
    private const string Window = "icon.ico";
    
    public static BitmapImage GetImage(IconType type) => new(new Uri(GeneratePath(type), UriKind.Relative));
    
    public static BitmapImage GetWindowImage() => new(new Uri(Window, UriKind.Relative));

    private static string GeneratePath(IconType type)
    {
        var imageFile = type switch
        {
            IconType.Back => Back,
            IconType.Executable => Window,
            IconType.Exit => Exit,
            IconType.Forward => Forward,
            IconType.New => New,
            IconType.Open => Open,
            IconType.Save => Save,
            IconType.Settings => Settings,
            _ => throw new Exception()
        };

        return $"{RelativePath}/{imageFile}";
    }
}
