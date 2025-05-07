using System.Data;
using OfficeOpenXml;
using CommandLine;
using Services;
using UserInterface;
using DocumentAnalysis;
using System.Drawing;
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
        [Option('e',"visual-example", HelpText = "create a visual analyzed example of an excel file, or all files within a directory",Default =null)]
        public string? visualExample{get; set;} = null;
        
    }
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
        {
            //For demonstration purpose, my personal NON COMMERCIAL account, this should OBVIOUSLY be replaced!!!!
            ExcelPackage.License.SetNonCommercialPersonal("Nikolaj R Christensen"); 
            
            SynonymDictionary synonyms;
            RegexAnalyzer.RegexAnalyzer stringAnalyzer;

            ConsoleUI CUI = new ConsoleUI();


            //Set up dictionary 
            try
            {
                synonyms = new (o.synonymDictionary,'\t',';',["..","nogen/noget","(",")"]);
                stringAnalyzer = new (synonyms);
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
                            stringAnalyzer.train(fileWriter,package,CUI);
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
                    stringAnalyzer.load(reader);

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
                //Clean up input
                o.visualExample=o.visualExample.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
                //Check how many files we are going to loop over
                Dictionary<string, string> FromTo=new();
                try
                {
                    FileAttributes attributes = File.GetAttributes(o.visualExample);
                    //If it is a directory, we want to copy the entire structure to output/
                    if ((attributes & FileAttributes.Directory)==FileAttributes.Directory)
                    {
                        string[] inputFiles = Directory.GetFiles(o.visualExample,"*.xlsx",SearchOption.AllDirectories);
                        foreach (string file in inputFiles )
                        {
                            FromTo[file]=Path.Combine("out",file.Replace(o.visualExample+Path.DirectorySeparatorChar,""));
                        }
                    }
                    else
                    {
                        if (!File.Exists(o.visualExample))
                        {
                            throw new ArgumentException("Fil "+o.visualExample+" ikke fundet!");
                        }
                        FromTo[o.visualExample]=Path.Combine("out",Path.GetFileName(o.visualExample));
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Kunne ikke finde filer i "+o.visualExample+", da der var et problem : "+ex.Message);
                    return;
                }

                DocumentAnalyzer documentAnalyzer = new(stringAnalyzer);

                //Ok, now start looping through the files
                Console.WriteLine("Analyzing files:");
                foreach (var pair in FromTo)
                {
                    Console.WriteLine(pair.Key+" : "+pair.Value);
                    try
                    {
                        //TEMPORARY, load an excel document and perform analysis
                        using (ExcelPackage package = new ExcelPackage(new FileInfo(pair.Key)))
                        {
                            documentAnalyzer.firstPass(package,true);
                            //VisualAnalyzer.vizualizeCellAnalysis(package,stringAnalyzer);
                            Console.WriteLine("Saving");
                            if (Path.GetDirectoryName(pair.Value)!=null)//I don't think the compiler warning here can be fixed, I think you can safely ignore it
                                Directory.CreateDirectory(Path.GetDirectoryName(pair.Value));
                            package.SaveAs(pair.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleColor original = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Springer over analysen af "+pair.Key+", da der var et problem : "+ex.Message);
                        Console.ForegroundColor = original;
                    }
                }
            }
        });


        return;
    }
}