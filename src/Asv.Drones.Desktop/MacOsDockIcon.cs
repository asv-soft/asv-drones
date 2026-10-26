using System.Runtime.InteropServices;

namespace Asv.Drones.Desktop;

internal static class MacOsDockIcon
{
    private const string ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";

    public static void Apply()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "app-icon.png");
        var nativePath = SendString(
            GetClass("NSString"),
            GetSelector("stringWithUTF8String:"),
            path
        );
        var image = SendPointer(
            Send(GetClass("NSImage"), GetSelector("alloc")),
            GetSelector("initWithContentsOfFile:"),
            nativePath
        );
        if (image == IntPtr.Zero)
        {
            return;
        }

        var application = Send(GetClass("NSApplication"), GetSelector("sharedApplication"));
        SendVoid(application, GetSelector("setApplicationIconImage:"), image);
        Send(image, GetSelector("release"));
    }

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr GetClass(string name);

    [DllImport(ObjectiveCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr GetSelector(string name);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr Send(IntPtr receiver, IntPtr selector);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPointer(IntPtr receiver, IntPtr selector, IntPtr value);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendString(
        IntPtr receiver,
        IntPtr selector,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string value
    );

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void SendVoid(IntPtr receiver, IntPtr selector, IntPtr value);
}
