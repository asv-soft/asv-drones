using System;
using System.Diagnostics;
using UIKit;

namespace Asv.Drones.Gui.iOS;

public class Application
{

    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        AppArgs.Instance.TryParse(args);
        AppArgs.Instance.TryParseFile();
        // This is done to catch unhandled exceptions
        // https://docs.avaloniaui.net/ru/docs/concepts/unhandledexceptions
        try
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
        catch (Exception e)
        {
            if (Debugger.IsAttached) Debugger.Break();
        }
        finally
        {
            // Ensure the logs are flushed and archived before exiting the application
        }
    }
}