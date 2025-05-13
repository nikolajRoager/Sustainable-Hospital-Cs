
using System.Text.RegularExpressions;

namespace UserInterface
{
    /// <summary>
    /// A mock UI which just says yes (for unit testing)
    /// </summary>
    public class FauxUI : IUI
    {
        public FauxUI()
        {
        }

        /// <summary>
        /// Let the user pick an option of the options
        /// </summary>
        /// <param name="description"></param>
        /// <param name="options"></param>
        /// <returns>a number betwixt 0 and options.count()</returns>
        public int selectOption(string description, List<string> options)
        {
            //I assume 0 is the "default" option
            return 0;
        }

        /// <summary>
        /// Inform the user that stuff has happened
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>

        public void WriteLine(string line,bool warning=false)
        {
            //Nothing ...
        }

        public string ReadLine(string line,bool warning=false)
        {
            return "null";
        }
    }
}