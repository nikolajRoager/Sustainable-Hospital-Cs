namespace Services
{
    /// <summary>
    /// A fake File reader, which just parrots what it was initially told, for unit testing.
    /// </summary>
    public class FauxReader : IFileReader
    {
        List<string> lines;
        
        public FauxReader(IEnumerable<string> content)
        {
            lines=content.ToList();
        }

        /// <summary>
        /// Parrot back what we were told
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadLines()
        {
            return lines;
        }
    }

}