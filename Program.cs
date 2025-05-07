using System.Data;
using OfficeOpenXml;
using CommandLine;
using Services;
using UserInterface;
using DocumentAnalysis;
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
        [Option('v',"visual-example", HelpText = "excel file we will create a visual analyzed example of",Default =null)]
        public string? visualExample{get; set;} = null;
    }
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
        {
            //For demonstration purpose, my personal NON COMMERCIAL account, this should OBVIOUSLY be replaced!!!!
            ExcelPackage.License.SetNonCommercialPersonal("Nikolaj R Christensen"); 
            
            SynonymDictionary synonyms;
            RegexAnalyzer.RegexAnalyzer analyzer;

            ConsoleUI CUI = new ConsoleUI();


            //Set up dictionary 
            try
            {
                synonyms = new (o.synonymDictionary,'\t',';',["..","nogen/noget","(",")"]);
                analyzer = new (synonyms);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                return;
            }

            //Set up a file writer which writes to a persistent file
            string UserdataPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SustainableHospital");
            //Create the directory if it doesn't exist
            Directory.CreateDirectory(UserdataPath);
            string  SavedLibrary = Path.Combine(UserdataPath,"ProduktLibrary.json");


            if (o.trainMode)
            {

                //Set up dictionary 
                try
                {

                    FileWriter fileWriter = new(SavedLibrary);
                    string ExcelfilePath = "VareTypeBibliotek.xlsx";

                    if (!File.Exists(ExcelfilePath))
                    {
                        throw new ArgumentException("Fil "+ExcelfilePath+" ikke fundet!");
                    }
                    else
                        using (ExcelPackage package = new ExcelPackage(new FileInfo(ExcelfilePath)))
                        {
                            analyzer.train(fileWriter,package,CUI);
                            fileWriter.save();
                        }
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
                    FileReader reader = new(SavedLibrary);
                    analyzer.load(reader);

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            if (o.visualExample!=null)
            {
                try
                {
                    if (!File.Exists(o.visualExample))
                    {
                        throw new ArgumentException("Fil "+o.visualExample+" ikke fundet!");
                    }

                    //TEMPORARY, load an excel document and perform analysis
                    using (ExcelPackage package = new ExcelPackage(new FileInfo(o.visualExample)))
                    {
                        VisualAnalyzer.vizualizeCellAnalysis(package,analyzer);
                        Console.WriteLine("Saving");
                        package.SaveAs("visualExample.xlsx");
                    }
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