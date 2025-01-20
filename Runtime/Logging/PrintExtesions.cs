using DivineSkies.Modules.Logging;

public static class PrintExtesions
{
    public static void PrintMessage(this object obj, string message, bool onScreen = true) => Print(obj, message, onScreen ? MessageType.ScreenMessage : MessageType.LogMessage);
    public static void PrintLog(this object obj, object msgObj) => Print(obj, msgObj.ToString(), MessageType.Log);
    public static void PrintLog(this object obj, string message) => Print(obj, message, MessageType.Log);
    public static void PrintWarning(this object obj, object msgObj) => Print(obj, msgObj.ToString(), MessageType.Warning);
    public static void PrintWarning(this object obj, string message) => Print(obj, message, MessageType.Warning);
    public static void PrintError(this object obj, object msgObj) => Print(obj, msgObj.ToString(), MessageType.Error);
    public static void PrintError(this object obj, string message) => Print(obj, message, MessageType.Error);
    private static void Print(object obj, string message, MessageType type = MessageType.Log)
    {
        var className = obj.GetType().ToString().Remove(0, obj.GetType().ToString().LastIndexOf('.') + 1);
        Log.Main.PrintLogMessage(className, message, type);
    }
}