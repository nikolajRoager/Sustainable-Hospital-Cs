using System.Data;
using OfficeOpenXml;
using CommandLine;

static class Program
{
    public class Options
    {
        [Option('K',"synonym-key-sep", HelpText = "seperator between key-word and synonyms in dictionary",Default ='\t')]
        public char synonymSepKey {get; set;} ='\t';
        [Option('S',"synonym-syn-sep", HelpText = "seperator between synonyms in dictionary",Default =';')]
        public char synonymSepSyn {get; set;} =';';
        [Option('s',"synonyms", HelpText = "File with synonyms",Default ="ddo-synonyms.csv")]
        public string synonymDictionary {get; set;} ="ddo-synonyms.csv";
        
        [Option('l',"library", HelpText = "Excel file with products and categories",Default ="VareTypeBibliotek.xlsx")]
        public string productLibrary {get; set;} ="VareTypeBibliotek.xlsx";
        
        [Option('t',"train", HelpText = "Run in training mode; will be overwritten and forced to true if an input tabel is included")]
        public bool trainMode{get; set;} =false;

    }
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
        {
            //For demonstration purpose, my personal NON COMMERCIAL account, this should OBVIOUSLY be replaced!!!!
            ExcelPackage.License.SetNonCommercialPersonal("Nikolaj R Christensen"); 
            
            SynonymDictionary synonyms;
            RegexProductFinder.RegexProductLibrary regexProductLibrary;
            
            if (o.trainMode)
            {

                //Set up dictionary and library, maybe stop with errors
                try
                {
                    synonyms = new (o.synonymDictionary,'\t',';',["..","nogen/noget","(",")"]);
                    regexProductLibrary = new (o.productLibrary,synonyms);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            else
            {
                //Set up dictionary and library, maybe stop with errors
                try
                {
                    synonyms = new (o.synonymDictionary,'\t',';',["..","nogen/noget","(",")"]);
                    regexProductLibrary = new (synonyms);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    return;
                }

            }
        });
        return;
    }
}