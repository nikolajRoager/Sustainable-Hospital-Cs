using System.Data;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using OfficeOpenXml;
using FuzzySharp;

/// <summary>
/// A class which analyzes the user supplied excel files, and figures out where the columns are 
/// </summary>
class RegexExcelAnalyzer
{
    //Ideally, we will recognize what things are, based on the name given to the column, by searching for these kind of names:

    //Things to search for to find the column with that thing in, in prioritized order (i.e. if there are multiple matches, go with the first)
    public List<Regex> productNameCNames {get; set;}
    
    public List<Regex> NrCNames {get; set;}

    //I ASSUME KG is the default unit of mass, if anything else is use that ought to be specifie
    //If you really want to use something else ... you could just rename the name to pounds or whatever
    //One problem is that we do not, and can not know if vægt means unit-mass unit, or total mass
    public List<Regex> MassKgNames {get; set;}

    public List<Regex> QuantityCNames {get; set;}

    //A few words which indicate that something refers to totality, for example total mass
    public List<Regex> total {get; set;}
    public List<Regex> single {get; set;}

    /// <summary>
    /// For searching for mass, either in the header or in the name themself, these are a few different unit names and their mass in kg
    /// </summary>
    public Dictionary<Regex, double> MassUnitNames {get; set;}

    public RegexExcelAnalyzer(string ExcelRegexPath,SynonymDictionary dictionary)
    {
        if (!File.Exists(ExcelRegexPath))
        {
            throw new ArgumentException("Fil "+ExcelRegexPath+" ikke fundet!");
        }
        else
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(ExcelRegexPath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                
                productNameCNames = new();
                NrCNames  = new();
                MassKgNames  = new();
                QuantityCNames  = new();
                total  = new();
                single = new();
                MassUnitNames = new();

                //I am just assuming that the variables are at column 1, from row 2 to 6

                for (int y = 2; y<=7; ++y)
                {
                    string? VariableName = worksheet.Cells[y, 1].Value?.ToString();
                    
                    try
                    {
                        switch(VariableName)
                        {
                            default:
                                break;
                            case "NavnHeader":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        productNameCNames.Add(new Regex(str));

                                }
                                break;
                            case "NrHeader":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        NrCNames.Add(new Regex(str));

                                }
                                break;
                            case "MasseKgHeader":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        MassKgNames.Add(new Regex(str));

                                }
                                break;
                            case "AntalHeader":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        QuantityCNames.Add(new Regex(str));

                                }
                                break;
                            case "EnkelNavn":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        single.Add(new Regex(str));

                                }
                                break;
                            case "TotalNavn":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        total.Add(new Regex(str));

                                }
                                break;
                        }
                    }
                    catch(Exception Ex)
                    {
                        throw new ArgumentException("Kunne ikke indelæse REGEX, fejl "+Ex.Message);
                    }

                }
                //I also know this is where KG synonym regices are stored
                for (int y = 10; y<=worksheet.Dimension.End.Row; ++y)
                {
                    string? regex_str="null";
                    try
                    {
                    //We will paste the regex for this kg into another regex, which selects whatever is just before, because that is the way most western languages works
                    regex_str =@"\b\d+(?:\.\d+)?(?=\s?"+worksheet.Cells[y,1].Value.ToString()+@"\b)" ;
                    string? unit_str =worksheet.Cells[y,2].Value.ToString();
                    if (regex_str!=null && unit_str!=null)
                    {
                        if (double.TryParse(unit_str,out double result))
                        {
                            MassUnitNames.Add(new Regex(regex_str),result);
                        }
                        else
                        throw new ArgumentException("Kunne ikke over sætte Regex "+regex_str+" fordi enhed manglede");
                    }
                    else
                        throw new ArgumentException("Kunne ikke over sætte Regex fordi regex manglede");
                    }
                    catch(Exception Ex)
                    {
                        throw new ArgumentException("Kunne ikke over sætte Regex "+regex_str+" fejl: "+Ex.Message);
                    }
                }

                //the variable name is on the left,
            }
        }
    }
}