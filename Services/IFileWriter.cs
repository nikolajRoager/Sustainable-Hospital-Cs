namespace Services
{
    /// <summary>
    /// An interface for writing to the filesystem (or mocking it, in the context of unit tests)
    /// </summary>
    public interface IFileWriter
    {
        /// <summary>
        /// Write one line to the file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public void writeLine(string line);
        /// <summary>
        /// Save to file (doesn't do anything if this is a mock filewriter), may throw errors
        /// </summary>
        public void save();
    }
}