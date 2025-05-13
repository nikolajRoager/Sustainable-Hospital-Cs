namespace UserInterface
{
    /// <summary>
    /// An interface for the user interface (or a mocked) which can present information to the user, or ask for information
    /// </summary>
    public interface IUI
    {
        /// <summary>
        /// Let the user pick an option of the options
        /// </summary>
        /// <param name="description"></param>
        /// <param name="options"></param>
        /// <returns>a number betwixt 0 and options.count()</returns>
        public int selectOption(string description, List<string> options);

        /// <summary>
        /// Inform the user that stuff has happened
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>

        public void WriteLine(string line,bool warning=false);

        /// <summary>
        /// Inform the user that they need to write stuff
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>

        public string ReadLine(string line,bool warning=false);
    }
}