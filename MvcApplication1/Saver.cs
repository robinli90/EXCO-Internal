using System.IO;

namespace MvcApplication1
{
    public static class Saver
    {
        public static readonly object _locker = new object();

        public static void Save(string msg, string filePath)
        {

            TextWriter writer = new StreamWriter(filePath);
            StreamToFile(writer, msg);

            if (writer != null)
            {
                writer.Flush();
                writer.Dispose();
                writer.Close();
            }
        }

        public static void StreamToFile(TextWriter sw, string msg)
        {
            sw.Write(msg);
        }
    }

}