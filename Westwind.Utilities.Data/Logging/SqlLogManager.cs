
using Westwind.Utilities.Logging;

public class SqlLogManager
{
    /// <summary>
    /// Creates an instance of the LogManager based on the
    /// settings configured in the web.config file
    /// </summary>        
    /// <returns></returns>
    public static LogManager Create(LogAdapterTypes logType)
    {
        ILogAdapter adapter = null;

        if (logType == LogAdapterTypes.Sql)
            adapter = new SqlLogAdapter();
        else if (logType == LogAdapterTypes.Xml)
            adapter = new XmlLogAdapter();
        else if (logType == LogAdapterTypes.Text)
            adapter = new TextLogAdapter();

        return Create(adapter);
    }
}