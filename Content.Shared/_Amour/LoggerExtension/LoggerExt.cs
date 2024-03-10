namespace Content.Shared._Amour.LoggerExtension;

public static class LoggerExt
{
    public static void Trace(this ISawmill logger, params (string,object)[] objects)
    {
        if(objects.Length == 0)
            return;

        var text = "TRC:  ";

        foreach (var (name,obj) in objects)
        {
            text += $"{name}: {obj}    ";
        }

        logger.Debug(text);
    }
}
