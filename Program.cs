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
        [Option('f',"file-or-folder", HelpText = "file or folder to analyze, by defeault creates a visual analyzed for the first pass example of an excel file, or all files within a directory",Default =null)]
        public string? fileOrFolder{get; set;} = null;
        [Option('n',"no-add", HelpText = "Do not allow adding new products to training data while running",Default =false)]
        public bool noAdd {get; set;} = false;
        
        
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

            //Set up a file writer which writes to a persistent file
            string UserdataPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SustainableHospital");
            //Create the directory if it doesn't exist
            Directory.CreateDirectory(UserdataPath);
            string  SavedLibrary = Path.Combine(UserdataPath,"ProduktLibrary.json");

            //Set up dictionary and regex analyzer
            try
            {
                synonyms = new (o.synonymDictionary,'\t',';',["..","nogen/noget","(",")"]);
                stringAnalyzer = new (synonyms,CUI);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                return;
            }


            if (o.trainMode)
            {

                //Set up dictionary 
                try
                {

                    string ExcelfilePath = o.productLibrary;

                    if (!File.Exists(ExcelfilePath))
                    {
                        throw new ArgumentException("Fil "+ExcelfilePath+" ikke fundet!");
                    }
                    else
                        using (ExcelPackage package = new ExcelPackage(new FileInfo(ExcelfilePath)))
                        {
                            FileWriter fileWriter = new(SavedLibrary);
                            stringAnalyzer.train(package,fileWriter );
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
                    FileReader fileReader= new(SavedLibrary);
                    stringAnalyzer.load(fileReader);

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            if (o.fileOrFolder!=null)
            {
                //Clean up input
                o.fileOrFolder=o.fileOrFolder.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
                //Check how many files we are going to loop over
                Dictionary<string, string> FromTo=new();
                try
                {
                    FileAttributes attributes = File.GetAttributes(o.fileOrFolder);
                    //If it is a directory, we want to copy the entire structure to output/
                    if ((attributes & FileAttributes.Directory)==FileAttributes.Directory)
                    {
                        string[] inputFiles = Directory.GetFiles(o.fileOrFolder,"*.xlsx",SearchOption.AllDirectories);
                        foreach (string file in inputFiles )
                        {
                            FromTo[file]=Path.Combine("out",file.Replace(o.fileOrFolder+Path.DirectorySeparatorChar,""));
                        }
                    }
                    else
                    {
                        if (!File.Exists(o.fileOrFolder))
                        {
                            throw new ArgumentException("Fil "+o.fileOrFolder+" ikke fundet!");
                        }
                        FromTo[o.fileOrFolder]=Path.Combine("out",Path.GetFileName(o.fileOrFolder));
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Kunne ikke finde filer i "+o.fileOrFolder+", da der var et problem : "+ex.Message);
                    return;
                }

                DocumentAnalyzer documentAnalyzer = new(stringAnalyzer,CUI);

                //Ok, now start looping through the files
                Console.WriteLine("Analysere fil:");
                foreach (var pair in FromTo)
                {
                    Console.WriteLine(pair.Key+" : "+pair.Value);
                    try
                    {
                        //TEMPORARY, load an excel document and perform analysis
                        using (ExcelPackage package = new ExcelPackage(new FileInfo(pair.Key)))
                        {

                            documentAnalyzer.Analyze(package,true,o.noAdd);
                            //VisualAnalyzer.vizualizeCellAnalysis(package,stringAnalyzer);
                            Console.WriteLine("Gemmer");
                            if (pair.Value!=null)//I don't think the compiler warning here can be fixed, I think you can safely ignore it
                            {
                                var dirName = Path.GetDirectoryName(pair.Value);
                                if (dirName!=null)
                                    Directory.CreateDirectory(dirName);
                            }
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

                if (stringAnalyzer.isModified)
                {
                    if (CUI.selectOption("Skal vi gemme ændringerne i træningsdata", [$"Ja, gem i {SavedLibrary}", "Nej"])==0)
                    {
                        FileWriter fileWriter = new(SavedLibrary);
                        stringAnalyzer.save(fileWriter );
                        fileWriter.save();
                    }
                }
            }
        });
        return;
    }
}