namespace Services
{
    /// <summary>
    /// File reader, which can reads data from the computers filesystem
    /// </summary>
    class FileReader : IFileReader
    {
        string path;
        
        public FileReader(string path)
        {
            this.path = path;
            //If this is input, the intput file MUST exist, otherwise throw an error
            if (!(Path.Exists(path) && new FileInfo(path).Length > 0))
            {
                throw new FileNotFoundException("Fil "+path+" ikke fundet!");
            }
        }

        /// <summary>
        /// Get enumerable to access the lines of the file
        /// Might Throws IOException, but only if the file has been changed/deleted between the initial check and the call of this function
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> readLines()
        {
            return File.ReadLines(path);
        }
    }

}