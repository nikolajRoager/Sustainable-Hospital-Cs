namespace UserInterface
{
    /// <summary>
    /// An interface for the user interface (or a mocked) which can present information to the user, or ask for information
    /// </summary>
    public class ConsoleUI : IUI
    {
        private ConsoleColor defaultColor;
        private ConsoleColor warningColor;

        public ConsoleUI()
        {
            defaultColor = Console.ForegroundColor;
            warningColor = ConsoleColor.Yellow;
        }

        /// <summary>
        /// Let the user pick an option of the options
        /// </summary>
        /// <param name="description"></param>
        /// <param name="options"></param>
        /// <returns>a number betwixt 0 and options.count()</returns>
        public int selectOption(string description, List<string> options)
        {
            Console.ForegroundColor=warningColor;
            Console.Write("Advarsel:");
            Console.ForegroundColor=defaultColor;
            Console.WriteLine(" "+description);
            for (int i = 0; i < options.Count; ++i)
            {Console.WriteLine($"\t({i}) : "+options[i]);}
            Console.WriteLine("Skriv tal; Afslut med enter: ");

            //Keep polling for inputs till we get something valid
            int number = options.Count;
            while (number<0 || number >= options.Count)
            {
                string? input = Console.ReadLine();
                if (input==null)
                {continue;}

                if (!int.TryParse(input,out number))
                    number = options.Count; 
            }

            return number;

        }

        /// <summary>
        /// Inform the user that stuff has happened
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>

        public void WriteLine(string line,bool warning=false)
        {
            if (warning)
            {
                Console.ForegroundColor= warningColor;
                Console.Write("Advarsel: ");
                Console.ForegroundColor= defaultColor;
            }
            Console.WriteLine(line);
        }

        public string ReadLine(string line,bool warning=false)
        {
            if (warning)
            {
                Console.ForegroundColor= warningColor;
                Console.Write("Advarsel: ");
                Console.ForegroundColor= defaultColor;
            }
            Console.WriteLine(line);
            string? input = Console.ReadLine();
            return input==null?"":input;
        }
    }
}