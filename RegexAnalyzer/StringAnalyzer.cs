
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography.Pkcs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FuzzySharp;
using OfficeOpenXml;
using Services;
using StringAnalyzer;
using UserInterface;
namespace RegexAnalyzer
{
    /// <summary>
    /// The default constructor creates an empty analyzer without any training
    /// </summary>
    /// <param name="dictionary"></param>
    public class RegexAnalyzer(SynonymDictionary dictionary) : IStringAnalyzer
    {

    //Integer, not part of decimal using negative lookbehind
        static Regex findInteger = new Regex(@"(?<![\d])\-?(?:\d{1,3}(?:\.\d{3})+|\d+)(?!.\d)",RegexOptions.IgnoreCase);
    //Decimal marker .;  , may be used to divide thousands (optional)
        static Regex findDecimal =new Regex(@"(?<![\d])\-?(?:\d{1,3}(?:,\d{3})+|\d+).?\d+(?!\d)",RegexOptions.IgnoreCase);

        //Product numbers occationally include a - in the middle
        static Regex findProductNr = new Regex(@"\d+\-\d+",RegexOptions.IgnoreCase);
        /*Try it out on this example:
            23020-23423 454 first is a product number
            -324324 not a product number
            23453 not a product number
        */
        
        static Regex findAmount = new Regex(@"(?<![\d:-])\d+",RegexOptions.IgnoreCase);
        /*Try it out on this example:
            23020-23423 454 first is a product number
            -324324 not a product number
            23453 not a product number
        */
        
        //I could have made the code shorter by using a dictionary<string,set<regex>> where the string is the header type
        //But I think the code is easier to understand conceptually if I write it out like that
        //Things to search for to find the column with that thing in, in prioritized order (i.e. if there are multiple matches, go with the first)
        public List<Regex> ProductHeaders {get; set;} = new List<Regex>();
        
        public List<Regex> NrHeaders {get; set;} = new List<Regex>();

        //I ASSUME KG is the default unit of mass, if anything else is use that ought to be specifie
        //If you really want to use something else ... you could just rename the name to pounds or whatever
        //One problem is that we do not, and can not know if vægt means unit-mass unit, or total mass
        public List<Regex> MassHeaders {get; set;} = new List<Regex>();

        public List<Regex> QuantityHeaders {get; set;} = new List<Regex>();

        //A few words which indicate that something refers to totality, for example total mass
        public List<Regex> TotalRegices {get; set;} = new List<Regex>();
        public List<Regex> SingleRegices {get; set;} = new List<Regex>();

        /// <summary>
        /// For searching for mass, either in the header or in the name themself, these are a few different unit names and their mass in kg
        /// </summary>
        public Dictionary<Regex, double> MassUnitNames {get; set;} = new();



        SynonymDictionary synonymDictionary=dictionary;
        /// <summary>
        ///The product pairs, sorted by priority, so highest priority keywords come first 
        /// </summary>
        public SortedSet<RegexProduct> Products=new();

        /// <summary>
        /// Sets with all categories added, used to check for duplicates/synonyms
        /// </summary>
        HashSet<string> categories=new();

        /// <summary>
        /// Sets with all Materials added, used to check for duplicates/synonyms
        /// </summary>
        HashSet<string> Materials=new();

        
        public AnalyzedString Analyze(string? cell_input)
        {
            //The steps to analyzing a string is the following: 
            //Check if it is empty (break early),
            //Check if it is only a single number (break early),
            //Check if the string matches any known header,
            //Check if the string matches any known product
            //Check if the string contains mass, quantity, or product number


            if (string.IsNullOrEmpty(cell_input))
                return new AnalyzedString();//Returns that this is 100% guaranteed to be filler

            //Check if it is just a plain integer or a plain decimal
            var style = System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint;
            var culture = CultureInfo.InvariantCulture;
            if (int.TryParse(cell_input, style, culture, out int myIntValue))
            {
                return new AnalyzedString(myIntValue,cell_input);
            }
            else if (double.TryParse(cell_input, style, culture, out double myDoubleValue))
            {
                return new AnalyzedString(myDoubleValue,cell_input);
            }
            //Ok, now check against all known headers, one at the time
            //I could have made the code shorter by using a dictionary<string,set<regex>> where the string is the header type
            //But I think the code is easier to understand conceptually if I write it out like that

            bool productHeader_found = false;
            bool NrHeader_found = false;
            bool MassHeader_found = false;
            bool QuantityHeader_found = false;


            //If this product name header?
            foreach (var header in ProductHeaders)
                if (header.IsMatch(cell_input))
                {
                    productHeader_found = true;
                    break;
                }
            foreach (var header in NrHeaders)
                if (header.IsMatch(cell_input))
                {
                    NrHeader_found= true;
                    break;
                }
            foreach (var header in MassHeaders)
                if (header.IsMatch(cell_input))
                {
                    MassHeader_found= true;
                    break;
                }
            foreach (var header in QuantityHeaders)
                if (header.IsMatch(cell_input))
                {
                    QuantityHeader_found= true;
                    break;
                }

            //Now, even if this can be a header, check if it could also be a product           
            bool ProductMatch_found = false;
            foreach (var product in Products)
                if (product.KeyRegex.IsMatch(cell_input))
                {
                    ProductMatch_found= true;
                    break;
                }
            
            //It could also contain a product number
            bool ProductNrMatch_found = false;
            var pmatch = findProductNr.Match(cell_input);
            string productNrMatch="";            
            if (pmatch.Success)
            {
                ProductNrMatch_found = true;
                productNrMatch=pmatch.Value;
            }

            //Oh, and check if there is an amount somewhere
        
            bool AmountMatch_found = false;
            var amatch = findAmount.Match(cell_input);
            int amount=0;
            if (amatch.Success)
                if (int.TryParse(amatch.Value, style, culture,out amount))
                {
                    //It should be a double, if not ignore it
                    AmountMatch_found = true;
                }

            //Any indication this has to do with single or multiple
            bool singleFound = false;

            foreach (Regex Single in SingleRegices)
            {
                if (Single.IsMatch(cell_input))
                {
                    singleFound=true;
                    break;
                }
            }
            bool totalFound = false;
            foreach (Regex Total in TotalRegices)
            {
                if (Total.IsMatch(cell_input))
                {
                    totalFound =true;
                    break;
                }
            }

            //Then let us check if there are any words indicating it might be mass either per unit or in total
            double mass=0;
            bool mass_found=false;

            //And finally, check if there are masses directly included in the text
            foreach ((Regex massRegex,double massKg) in MassUnitNames)
            {
                var match =(massRegex.Match(cell_input));
                if (match.Success)
                {
                    if (!double.TryParse(match.Value, style, culture,out mass))
                    {
                        //It should be a double, if not ignore it
                        continue;
                    }
                    //Convert to kg if need be
                    mass*=massKg;
                    mass_found=true;
                    break;
                }
            }

            //Ok, now combine everything, if nothing was found, it is just filler
            if (!mass_found && !AmountMatch_found && ! productHeader_found && !NrHeader_found && !MassHeader_found &&  !ProductMatch_found && !ProductNrMatch_found && !QuantityHeader_found)
                return new AnalyzedString(cell_input);
            
            //Otherwise, let us create an analyzed string with our best estimates
            return new AnalyzedString{
                content=cell_input,
                filler=1,//Filler is always an option
                //Give both the same weight
                //If singlefound == totalfound, the mass can be either or,
                containsTotalMass=(mass_found ) ? ((singleFound ==totalFound) ? 10 : (totalFound) ? 10 : 0) :0,
                containsSingleMass=(mass_found ) ? ((singleFound ==totalFound) ? 10 : (singleFound) ? 10 : 0) :0,
                isInteger =0,
                isDecimal= 0,
                intValue =amount,
                doubleValue=mass,
                productNameHeader=productHeader_found?10:0,//Give any headers found same weight
                NrHeader=NrHeader_found?10:0,
                SingleMassHeader=MassHeader_found? ((singleFound ==totalFound) ? 10 : (singleFound) ? 10 : 0) : 0,
                TotalMassHeader=MassHeader_found ? ((singleFound ==totalFound) ? 10 : (totalFound) ? 10 : 0) : 0,
                QuantityHeader=QuantityHeader_found ?10:0,    
                containsAmount=AmountMatch_found ?10:0,
                containsProduct=ProductMatch_found?10:0,
                containsProductNr=ProductNrMatch_found?10:AmountMatch_found?5:0,//Something which looks like amount can also be product number
                ProductNr= ProductNrMatch_found?productNrMatch:AmountMatch_found?$"{amount}":"null"//The "amount" might be a product number
            };
        }


        /// <summary>
        /// Run the analysis on the input, and spit out what we think it might be, with higher quality but likely slower, likely including fuzzy and synonym search
        /// though implementation may differ
        /// This implementation is the same as the less detailed, but with some more matches and searches
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public AnalyzedString AnalyzeDetailed(string? cell_input)
        {
            //The steps to analyzing a string is the following: 
            //Check if it is empty (break early), it won't be because we likely already did the less detailed part
            //Check if it is only a single number (break early), again, likely not
            //Check if the string matches any known header,
            //Check if the string matches any known product
            //Check if the string contains mass, quantity, or product number


            if (string.IsNullOrEmpty(cell_input))
                return new AnalyzedString();//Returns that this is 100% guaranteed to be filler

            //Check if it is just a plain integer or a plain decimal
            var style = System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint;
            var culture = CultureInfo.InvariantCulture;
            if (int.TryParse(cell_input, style, culture, out int myIntValue))
            {
                return new AnalyzedString(myIntValue,cell_input);
            }
            else if (double.TryParse(cell_input, style, culture, out double myDoubleValue))
            {
                return new AnalyzedString(myDoubleValue,cell_input);
            }
            //Ok, now check against all known headers, one at the time
            //I could have made the code shorter by using a dictionary<string,set<regex>> where the string is the header type
            //But I think the code is easier to understand conceptually if I write it out like that

            bool productHeader_found = false;
            bool NrHeader_found = false;
            bool MassHeader_found = false;
            bool QuantityHeader_found = false;


            //If this product name header?
            foreach (var header in ProductHeaders)
                if (header.IsMatch(cell_input))
                {
                    productHeader_found = true;
                    break;
                }
            foreach (var header in NrHeaders)
                if (header.IsMatch(cell_input))
                {
                    NrHeader_found= true;
                    break;
                }
            foreach (var header in MassHeaders)
                if (header.IsMatch(cell_input))
                {
                    MassHeader_found= true;
                    break;
                }
            foreach (var header in QuantityHeaders)
                if (header.IsMatch(cell_input))
                {
                    QuantityHeader_found= true;
                    break;
                }

            //Now, even if this can be a header, check if it could also be a product           
            bool ProductMatch_found = false;
            foreach (var product in Products)
                if (product.KeyRegex.IsMatch(cell_input))
                {
                    ProductMatch_found= true;
                    break;
                }
                else
                {
                    //The fuzzy search doesn't really work with regex, sadly, so split it into words and join it together
                    var keywordList = product.keyWordList();
                    if (Fuzz.PartialRatio(String.Join(" ", keywordList),cell_input)>80)
                    {
                        ProductMatch_found = true;
                        break;
                    }
                    
                }
            
            //It could also contain a product number
            bool ProductNrMatch_found = false;
            var pmatch = findProductNr.Match(cell_input);
            string productNrMatch="";            
            if (pmatch.Success)
            {
                ProductNrMatch_found = true;
                productNrMatch=pmatch.Value;
            }

            //Oh, and check if there is an amount somewhere
        
            bool AmountMatch_found = false;
            var amatch = findAmount.Match(cell_input);
            int amount=0;
            if (amatch.Success)
                if (int.TryParse(amatch.Value, style, culture,out amount))
                {
                    //It should be a double, if not ignore it
                    AmountMatch_found = true;
                }

            //Any indication this has to do with single or multiple
            bool singleFound = false;

            foreach (Regex Single in SingleRegices)
            {
                if (Single.IsMatch(cell_input))
                {
                    singleFound=true;
                    break;
                }
            }
            bool totalFound = false;
            foreach (Regex Total in TotalRegices)
            {
                if (Total.IsMatch(cell_input))
                {
                    totalFound =true;
                    break;
                }
            }

            //Then let us check if there are any words indicating it might be mass either per unit or in total
            double mass=0;
            bool mass_found=false;

            //And finally, check if there are masses directly included in the text
            foreach ((Regex massRegex,double massKg) in MassUnitNames)
            {
                var match =(massRegex.Match(cell_input));
                if (match.Success)
                {
                    if (!double.TryParse(match.Value, style, culture,out mass))
                    {
                        //It should be a double, if not ignore it
                        continue;
                    }
                    //Convert to kg if need be
                    mass*=massKg;
                    mass_found=true;
                    break;
                }
            }

            //Ok, now combine everything, if nothing was found, it is just filler
            if (!mass_found && !AmountMatch_found && ! productHeader_found && !NrHeader_found && !MassHeader_found &&  !ProductMatch_found && !ProductNrMatch_found && !QuantityHeader_found)
                return new AnalyzedString(cell_input);
            
            //Otherwise, let us create an analyzed string with our best estimates
            return new AnalyzedString{
                content=cell_input,
                filler=1,//Filler is always an option
                //Give both the same weight
                //If singlefound == totalfound, the mass can be either or,
                containsTotalMass=(mass_found ) ? ((singleFound ==totalFound) ? 10 : (totalFound) ? 10 : 0) :0,
                containsSingleMass=(mass_found ) ? ((singleFound ==totalFound) ? 10 : (singleFound) ? 10 : 0) :0,
                isInteger =0,
                isDecimal= 0,
                intValue =amount,
                doubleValue=mass,
                productNameHeader=productHeader_found?10:0,//Give any headers found same weight
                NrHeader=NrHeader_found?10:0,
                SingleMassHeader=MassHeader_found? ((singleFound ==totalFound) ? 10 : (singleFound) ? 10 : 0) : 0,
                TotalMassHeader=MassHeader_found ? ((singleFound ==totalFound) ? 10 : (totalFound) ? 10 : 0) : 0,
                QuantityHeader=QuantityHeader_found ?10:0,    
                containsAmount=AmountMatch_found ?10:0,
                containsProduct=ProductMatch_found?10:0,
                containsProductNr=ProductNrMatch_found?10:AmountMatch_found?5:0,//Something which looks like amount can also be product number
                ProductNr= ProductNrMatch_found?productNrMatch:AmountMatch_found?$"{amount}":"null"//The "amount" might be a product number
            };
        }
        

        //Everything below here is for saving, loading, and training

        /// <summary>
        /// For easy JSON serialization/deserialization, this helper class stores the things we would like to keep
        /// </summary>
        private class SerializableVariables
        {
            [JsonPropertyName("Products")]
            public List<RegexProduct> Products {get;set;} = null!;
            [JsonPropertyName("ProductHeaders")]
            public List<string> ProductHeaders {get;set;} = null!;
            [JsonPropertyName("ProductNumberHeaders")]
            public List<string> ProductNumberHeaders {get;set;} = null!;
            [JsonPropertyName("MassKgNames")]
            public List<string> MassKgNames {get;set;} = null!;
            [JsonPropertyName("QuantityHeaders")]
            public List<string> QuantityHeaders {get;set;} = null!;
            [JsonPropertyName("TotalRegices")]
            public List<string> TotalRegices {get;set;} = null!;
            [JsonPropertyName("SingleRegices")]
            public List<string> SingleRegices {get;set;} = null!;
            [JsonPropertyName("MassNames")]
            public List<string> MassNames {get;set;} = null!;
            [JsonPropertyName("MassUnits")]
            public List<double> MassUnits {get;set;} = null!;
        }

        public void train(IFileWriter PersistFile, ExcelPackage package, IUI UserInterface)
        {

            //What replacements do we need to make?
            Dictionary<string,string> CategoryReplacement = new Dictionary<string,string>();
            Dictionary<string,string> MaterialReplacement = new Dictionary<string,string>();
            
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            
            //The columns need to be here
            int header_start     = 1;
            int header_Category  = 1;
            int header_Material  = 2;
            int header_keyword   = 3;
            int header_MassUnit  = 4;
            int header_Kg        = 5;
            int header_nameHeader= 6;
            int header_nrHeader  = 7;
            int header_massHeader= 8;
            int header_quantityHeader  = 9;
            int header_Single    = 10;
            int header_Total     = 11;

            //Read the individual variables, until the column becomes empty

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_nameHeader].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                ProductHeaders.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_nrHeader].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                NrHeaders.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_massHeader].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                MassHeaders.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_quantityHeader].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                QuantityHeaders.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_Total].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                TotalRegices.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var Cell=worksheet.Cells[i, header_Single].Value;
                var CellString = Cell?.ToString();
                if (CellString==null ||CellString.Length==0)
                    break;
                SingleRegices.Add(new Regex(CellString,RegexOptions.IgnoreCase));
            }

            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                var UnitCell=worksheet.Cells[i, header_MassUnit].Value;
                var KgCell=worksheet.Cells[i, header_Kg].Value;
                var UnitCellString = UnitCell?.ToString();
                var KgCellString = KgCell?.ToString();
                if (UnitCellString==null ||UnitCellString.Length==0||KgCellString==null ||KgCellString.Length==0)
                    break;

                //This regex matches the decimal part of any 'decimal kg' patterns, for example, try it on these:
                /*
                    90kg
                    90.0kg
                    6 90.0 kg 5-343
                */   
                MassUnitNames[new Regex(@"\b\d+(?:\.\d+?)?(?=\s?"+UnitCellString+@"\b)",RegexOptions.IgnoreCase)] = double.Parse(KgCellString);
            }

            /*
        public Dictionary<Regex, double> MassUnitNames {get; set;} = new();
            */
                
            Products = new();
            for (int i = header_start+1; i <= worksheet.Dimension.End.Row; i++)
            {
                //There are many ways an entry can register as empty, either one of the cell or the string value will register as null, or it will have length 0
                //We wil throw an exception if any of those happen
                var catCell=worksheet.Cells[i, header_Category].Value;
                if (catCell==null)
                {
                    UserInterface.WriteLine($"Kategori manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                var catString=catCell.ToString();
                if (catString==null)
                {
                    UserInterface.WriteLine($"Kategori manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                var ingCell=worksheet.Cells[i, header_Material].Value;
                if (ingCell==null)
                {
                    UserInterface.WriteLine($"Råvare manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                var ingString=ingCell.ToString();
                if (ingString==null)
                {
                    UserInterface.WriteLine($"Råvare manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                var keyCell=worksheet.Cells[i, header_keyword].Value;
                if (keyCell==null)
                {
                    UserInterface.WriteLine($"Søgeord manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                var keyString=keyCell.ToString();
                if (keyString==null)
                {
                    UserInterface.WriteLine($"Søgeord manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                if (ingString.Length==0)
                {
                    UserInterface.WriteLine($"Råvare manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                if (keyString.Length==0)
                {
                    UserInterface.WriteLine($"Søgeord manglede i række {i}, ignorerer linje",true);
                    continue;
                }
                if (catString.Length==0)
                {
                    UserInterface.WriteLine($"Kategori manglede i række {i}, ignorerer linje",true);
                    continue;
                }


                RegexProduct newPair;
                
                try
                {
                    newPair = new RegexProduct(catString.Trim(),ingString.Trim(),keyString.Trim());
                }
                catch (Exception ex)
                {
                    UserInterface.WriteLine($"Kunne ikke læse Regex "+keyString+$" på linje {i}, fejlmeddelelse: "+ex.Message+", ignorerer linje",true);
                    continue;
                }

                //Check if Material or Category need to be replaced

                //If this is a thing we already have been told to replace, use the other version
                if (MaterialReplacement.ContainsKey(newPair.Material))
                    newPair.Material=MaterialReplacement[newPair.Material];
                if (!Materials.Contains(newPair.Material))
                {
                    bool alreadyExists = false;
                    var synonyms = synonymDictionary.getSimilar(newPair.Material);
                    foreach (var synonym in synonyms)
                    {
                        if (Materials.Contains(synonym))
                        {
                            int number = UserInterface.selectOption("Den automatiske tekstgenkendelse er i tvivl om råvarene "+newPair.Material+" og "+synonym+$" er det samme (Fra linje {i} er produktet \""+newPair.Keyword+"\" med råvare \""+newPair.Material+")",["opret "+newPair.Material+" som ny råvare.\nSkriv tal, afslut med Enter","erstat råvare "+newPair.Material+" med "+synonym,"erstat råvare "+synonym+" med "+newPair.Material]);

                            if (number==2)
                            {
                                alreadyExists = true;
                                //This will replace all future
                                MaterialReplacement.Add(newPair.Material,synonym);
                                newPair.Material=synonym;
                            }
                            else if (number==3)
                            {
                                //This will replace all future
                                MaterialReplacement.Add(synonym,newPair.Material);
                                //Also replace all old versions of the old
                                foreach(var oldPair in Products)
                                    if (oldPair.Material==synonym)
                                        oldPair.Material=newPair.Material;
                                Materials.Remove(synonym);
                            }
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        Materials.Add(newPair.Material);
                    }
                }
                //If this is a thing we already have been told to replace, use the other version
                if (CategoryReplacement.ContainsKey(newPair.Category))
                    newPair.Category=CategoryReplacement[newPair.Category];
                if (!categories.Contains(newPair.Category))
                {
                    bool alreadyExists = false;
                    var synonyms = synonymDictionary.getSimilar(newPair.Category);
                    foreach (var synonym in synonyms)
                    {
                        if (categories.Contains(synonym))
                        {
                    UserInterface.WriteLine("Den automatiske tekstgenkendelse er i tvivl om kategorierne "+newPair.Category+" og "+synonym+$" er det samme.\nPå linje {i} er produktet \""+newPair.Keyword+"\" med kategori \""+newPair.Category+"\".\nHvad skal vi gøre?\n  1 erstat kategori "+newPair.Category+" med "+synonym+"\n  2 erstat kategori "+synonym+" med "+newPair.Category+"\n  3 opret "+newPair.Category+" som ny kategori.\nSkriv tal, afslut med Enter");
                            int number = UserInterface.selectOption("Den automatiske tekstgenkendelse er i tvivl om råvarene "+newPair.Material+" og "+synonym+$" er det samme (Fra linje {i} er produktet \""+newPair.Keyword+"\" med råvare \""+newPair.Material+")",["opret "+newPair.Material+" som ny råvare.\nSkriv tal, afslut med Enter","erstat råvare "+newPair.Material+" med "+synonym,"erstat råvare "+synonym+" med "+newPair.Material]);

                            if (number==2)
                            {
                                alreadyExists = true;
                                //This will replace all future
                                MaterialReplacement.Add(newPair.Material,synonym);
                                newPair.Material=synonym;
                            }
                            else if (number==3)
                            {
                                //This will replace all future
                                MaterialReplacement.Add(synonym,newPair.Material);
                                //Also replace all old versions of the old
                                foreach(var oldPair in Products)
                                    if (oldPair.Material==synonym)
                                        oldPair.Material=newPair.Material;
                                Materials.Remove(synonym);
                            }
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        categories.Add(newPair.Category);
                    }
                }
                //Because of the compare functions in the pair, this only looks for identical keyword
                //Just tell the human that it happened, there is no way the program can know which pair is correct.
                if (Products.Contains(newPair ))
                {
                    UserInterface.WriteLine("\""+newPair.Keyword+"\" duplikeret!, ignorer seneste indgang",true);
                }
                else
                    Products.Add(newPair);
            }

            //Now save this to the persistent file as json
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented=true,//This is just nicer to look at when debugging
            };


            SerializableVariables variables = new SerializableVariables {
                Products=Products.ToList(),
                ProductHeaders=ProductHeaders.Select(regex => regex.ToString()).ToList(),
                ProductNumberHeaders=NrHeaders.Select(regex => regex.ToString()).ToList(),
                MassKgNames =MassHeaders.Select(regex => regex.ToString()).ToList(),
                QuantityHeaders=QuantityHeaders.Select(regex => regex.ToString()).ToList(),
                TotalRegices=TotalRegices.Select(regex => regex.ToString()).ToList(),
                SingleRegices=SingleRegices.Select(regex => regex.ToString()).ToList(),
                MassNames=MassUnitNames.Keys.Select(regex => regex.ToString()).ToList(),
                MassUnits=MassUnitNames.Values.ToList()
            };

            
            PersistFile.writeLine(JsonSerializer.Serialize(variables,jsonOptions));
        }


        /// <summary>
        /// Load from persistent file
        /// </summary>
        /// <param name="PersistReader"></param>
        /// <exception cref="Exception"></exception>
        public void load(IFileReader PersistReader)
        {  
            string jsonString = "";
            foreach (string line in PersistReader.ReadLines())
            {
                jsonString += line;
            }
            
            SerializableVariables? variables = JsonSerializer.Deserialize<SerializableVariables>(jsonString);

            if (variables == null)
            {
                throw new Exception("Kunne ikke indlæse persistente indstillinger; kør i trænings modus først!");
            }

            Products=new();
            foreach (var p in variables.Products)
                Products.Add(p);

            ProductHeaders=variables.ProductHeaders.Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            NrHeaders =variables.ProductNumberHeaders.Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            MassHeaders =variables.MassKgNames .Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            QuantityHeaders =variables.QuantityHeaders.Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            TotalRegices =variables.TotalRegices.Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            SingleRegices =variables.SingleRegices.Select(regex => new Regex(regex,RegexOptions.IgnoreCase)).ToList();
            MassUnitNames=new();
            for (int i = 0; i < variables.MassUnits.Count; ++i)
                MassUnitNames[new Regex(variables.MassNames[i],RegexOptions.IgnoreCase)]=variables.MassUnits[i];

        }
    }
}
