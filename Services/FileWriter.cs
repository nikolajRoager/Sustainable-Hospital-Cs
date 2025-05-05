
namespace Services
{
    /// <summary>
    /// A faux file writer which doesn't actually write 
    /// </summary>
    public class FileWriter : IFileWriter
    {
        public List<string> lines {get;}
        public string FileName {get;}

        /// <summary>
        /// Warning, by design, we only throw IO errors on save, not on the constructor if we do not have permission to write
        /// </summary>
        /// <param name="FileName"></param>
        public FileWriter(string FileName)
        {
            lines = new List<string>();
            this.FileName = FileName;
        }



        /// <summary>
        /// Write one line to the file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public void writeLine(string line)
        {
            lines.Add(line);
        }
        /// <summary>
        /// Save to file
        /// </summary>
        public void save()
        {
            //Get the path
            string? path = new FileInfo(FileName)?.Directory?.FullName;

            if (path == null)
            {
                throw new FileNotFoundException("Foldernavnet til "+FileName+" kunne ikke findes; tjek navnet for fejl.");
            }

            //Create the directory if it doesn't exist, throws IO errors if we do not have write permission
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            //Save to a file so we can easilly load it again
            using (StreamWriter writer = new StreamWriter(FileName))
            {
                //Write every line to the file
                foreach (string line in lines )
                    writer.WriteLine(line);
            }
        }
    }
}