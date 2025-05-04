using System.Data;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using OfficeOpenXml;
using FuzzySharp;
using System;
using System.Globalization;
using RegexProductFinder;

/// <summary>
/// A class which analyzes excel strings, and guesses whether the string is a part of a header, empty, or if it contains product names, mass, quantity or generic numbers
/// </summary>
class RegexStringAnalyzer
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
    public List<Regex> totalRegices {get; set;}
    public List<Regex> singleRegices {get; set;}

    /// <summary>
    /// For searching for mass, either in the header or in the name themself, these are a few different unit names and their mass in kg
    /// </summary>
    public Dictionary<Regex, double> MassUnitNames {get; set;}


//Integer, not part of decimal using negative lookbehind
    public static Regex findInteger = new Regex(@"(?<![\d])\-?(?:\d{1,3}(?:\.\d{3})+|\d+)(?!.\d)");
//Decimal marker .;  , may be used to divide thousands (optional)
    public static Regex findDecimal =new Regex(@"(?<![\d])\-?(?:\d{1,3}(?:,\d{3})+|\d+).?\d+(?!\d)");
    
    private SynonymDictionary dictionary;
    private RegexProductLibrary library;

    public RegexStringAnalyzer(string ExcelRegexPath,SynonymDictionary dictionary, RegexProductLibrary lib)
    {
        this.dictionary = dictionary;
        this.library = lib;
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
                totalRegices  = new();
                singleRegices = new();
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
                                        singleRegices.Add(new Regex(str));

                                }
                                break;
                            case "TotalNavn":
                                for (int x = 2; x<worksheet.Dimension.End.Column && worksheet.Cells[y,x].Value!=null;++x)
                                {
                                    string? str = worksheet.Cells[y,x].Value.ToString();
                                    if (str!=null)
                                        totalRegices.Add(new Regex(str));

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
                    //For example if there is 5kg we select 5 (if the string is kg)
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


    public AnalyzedString Analyze(string input)
    {
        //The steps to analyzing a string is the following: 
        //Check if it is empty (break early),
        //Check if it is only a single number (break early),
        //Check if the string matches any known header,
        //Check if the string matches any known product
        //Check if the string contains mass pattern with number


        if (string.IsNullOrEmpty(input))
            return new AnalyzedString();//Returns 100% filler

        //Check if it is just a plain integer or a plain decimal
        var style = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint;
        var culture = CultureInfo.InvariantCulture;
        if (int.TryParse(input, style, culture, out int myIntValue))
        {
            return new AnalyzedString(myIntValue);
        }
        else if (double.TryParse(input, style, culture, out double myDoubleValue))
        {
            return new AnalyzedString(myDoubleValue);
        }
        else
        {
            //Ok, now use regex to check if we match masses directly in the text
            foreach ((Regex massRegex,double massKg) in MassUnitNames)
            {
                var match =(massRegex.Match(input));
                if (match.Success)
                {
                    double mass;
                    if (!double.TryParse(match.Value, style, culture,out mass))
                    {
                        //It should be a double, if not ignore it
                        continue;
                    }
                    //Convert to kg if need be
                    mass*=massKg;
                    Console.WriteLine(input+"->"+mass+" kg");
                    //Then let us check if there are any words indicating it might be mass per unit
                    bool singleFound = false;

                    foreach (Regex Single in singleRegices)
                    {
                        if (Single.IsMatch(input))
                        {
                            singleFound=true;
                            break;
                        }
                    }
                    bool totalFound = false;

                    foreach (Regex Total in totalRegices)
                    {
                        if (Total.IsMatch(input))
                        {
                            totalFound =true;
                            break;
                        }
                    }

                    //Ok, also check if there are any of our target words in here
                    if (singleFound == totalFound)
                    {
                        return new AnalyzedString{
        filler=0,
        //Give both the same weight
        containsTotalMass=10,
        containsSingleMass=10,
        isInteger =0,
        isDecimal= 0,
        intValue =0,
        doubleValue=mass,
        ProductName=(int)mass,
        productNameHeader=0,
        NrHeader=0,
        SingleMassHeader=0,
        TotalMassHeader=0,
        QuantityHeader=0,    
                        };
                    }
                    else if (singleFound && !totalFound)
                    {
                        return new AnalyzedString{
        filler=0,
        //It could still be single, but propably not
        containsTotalMass=10,
        containsSingleMass=5,
        isInteger =0,
        isDecimal= 0,
        intValue =0,
        doubleValue=mass,
        ProductName=(int)mass,
        productNameHeader=0,
        NrHeader=0,
        SingleMassHeader=0,
        TotalMassHeader=0,
        QuantityHeader=0,    
                        };
                    }
                    
                    else if (!singleFound && totalFound)
                    {
                        return new AnalyzedString{
        filler=0,
        //It could still be single, but propably not
        containsTotalMass=5,
        containsSingleMass=10,
        isInteger =0,
        isDecimal= 0,
        intValue =0,
        doubleValue=mass,
        ProductName=(int)mass,
        productNameHeader=0,
        NrHeader=0,
        SingleMassHeader=0,
        TotalMassHeader=0,
        QuantityHeader=0,    
                        };
                    }
                    
                }
            }
        }

        //I guess it must be filler
        return new AnalyzedString();//Returns 100% filler
        
    }
}