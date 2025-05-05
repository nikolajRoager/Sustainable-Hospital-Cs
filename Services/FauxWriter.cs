namespace Services
{
    /// <summary>
    /// A faux file writer which doesn't actually write 
    /// </summary>
    public class FauxWriter : IFileWriter
    {
        public List<string> lines {get;}

        public FauxWriter()
        {
            lines = new List<string>();
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
        /// Save to file (doesn't do anything if this is a mock filewriter)
        /// </summary>
        public void save()
        {/*nothing*/}
    }
}