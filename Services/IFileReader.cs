namespace Services
{
    /// <summary>
    /// An interface for reading from the filesystem (or mocking it, in the context of unit tests)
    /// </summary>
    public interface IFileReader
    {
        public IEnumerable<string> readLines();
    }

}