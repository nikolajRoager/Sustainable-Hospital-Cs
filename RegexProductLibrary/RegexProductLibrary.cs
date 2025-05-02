using System.Data;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using OfficeOpenXml;
using FuzzySharp;

namespace RegexProductFinder
{
    /// <summary>
    /// Library of all products known to the program, can identify the category and igredient of a given keyword
    /// </summary>
    public class RegexProductLibrary
    {
        SynonymDictionary synonymDictionary;

        /// <summary>
        ///The product pairs, sorted by priority, so highest priority keywords come first 
        /// </summary>
        public SortedSet<RegexProductPair> ProductPairs;

        /// <summary>
        /// Sets with all categories added, used to check for duplicates/synonyms
        /// </summary>
        HashSet<string> ingredients;

        /// <summary>
        /// Sets with all ingredients added, used to check for duplicates/synonyms
        /// </summary>
        HashSet<string> categories;
        
        /// <summary>
        ///Load library from saved training file
        /// </summary>
        public RegexProductLibrary(SynonymDictionary synonymDictionary)
        {
            this.synonymDictionary = synonymDictionary;
            ingredients =new HashSet<string>();
            categories=new HashSet<string>();
            ProductPairs = new();

            //Now save this to userdata
            string UserdataPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SustainableHospital");
            string  SavedLibrary = Path.Combine(UserdataPath,"ProduktLibrary.tsv");

            if (!Path.Exists(SavedLibrary))
            {
                throw new ArgumentException("Fil "+SavedLibrary+" med indlæst produktbibliotek ikke; programmet skal køres i træning mode først.");
            }
            
            try
            {
            foreach (string line in File.ReadLines(SavedLibrary))
            {
                string[] splitted=line.Split('\t');

                RegexProductPair newPair;
                
                try
                {
                    newPair = new RegexProductPair(splitted[0],splitted[1],splitted[2]);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"  Kunne ikke læse Regex "+splitted[2]+$" fejlmeddelelse: "+ex.Message);
                }

                ProductPairs.Add(newPair);
                ingredients.Add(newPair.ingredient);
                categories.Add(newPair.category);
            }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Kunne ikke indlæse "+SavedLibrary+" : "+ex.Message);
            }
        }

        /// <summary>
        ///Load library from an Excel file
        /// </summary>
        public RegexProductLibrary(string filePath,SynonymDictionary synonymDictionary)
        {
            this.synonymDictionary = synonymDictionary;
            ConsoleColor OriginalColor = Console.ForegroundColor;

            ingredients =new HashSet<string>();
            categories=new HashSet<string>();

            //What replacements do we need to make?
            Dictionary<string,string> categoryReplacement = new Dictionary<string,string>();
            Dictionary<string,string> ingredientReplacement = new Dictionary<string,string>();

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = OriginalColor;
                throw new ArgumentException("Fil "+filePath+" ikke fundet!");
            }
            else
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    
                    int header_start     = 1;
                    int header_category  = 0;
                    int header_ingredient= 0;
                    int header_keyword   = 0;

                    //Regices to find the headers
                    Regex CategoryRegex = new Regex(@"\b\w*(kategori|category)\w*\b",RegexOptions.IgnoreCase);
                    Regex IngredientRegex = new Regex(@"\b\w*(råvare|ingrediens|ingredient|ware)\w*\b",RegexOptions.IgnoreCase);
                    Regex KeywordRegex = new Regex(@"\b\w*(søge|ord|search|key|word)\w*\b",RegexOptions.IgnoreCase);

                    for (header_start = worksheet.Dimension.Start.Row; header_start <= worksheet.Dimension.End.Row; header_start++)
                    {
                        for (int x = worksheet.Dimension.Start.Column; x <= worksheet.Dimension.End.Column; x++)
                        {
                            object cellValue = worksheet.Cells[header_start, x].Value;

                            if (cellValue != null && x != header_ingredient && x != header_category && x != header_keyword)
                            {
                                string? sv = cellValue.ToString();
                                if (sv != null)
                                {
                                    if ( CategoryRegex.IsMatch(sv.Trim()))
                                    {
                                        header_category=x;
                                    }
                                    else if (IngredientRegex.IsMatch(sv.Trim()))
                                    {
                                        header_ingredient=x;
                                    }
                                    else if (KeywordRegex.IsMatch(sv.Trim()))
                                    {
                                        header_keyword=x;
                                    }
                                }
                            }
                        }
                        //All headers must be found, or none
                        if (header_ingredient==0 || header_category==0 || header_keyword==0)
                        {
                            header_ingredient=0;
                            header_category=0;
                            header_keyword=0;
                        }
                        else if (header_ingredient!=0 && header_category!=0 && header_keyword!=0)
                        {
                            break;
                        }
                    }

                    if (header_ingredient==0 || header_category==0 || header_keyword==0)
                    {
                        Console.ForegroundColor = OriginalColor;
                        throw new ArgumentException("Tabel Header Søgeord, Kategori, og Råvare ikke fundet i "+filePath);
                    }


                    ProductPairs = new();
                    for (int i = header_start+1; i < worksheet.Dimension.End.Row; i++)
                    {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        //There are many ways an entry can register as empty, either one of the cell or the string value will register as null, or it will have length 0
                        //We wil throw an exception if any of those happen
                        var catCell=worksheet.Cells[i, header_category].Value;
                        if (catCell==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   Kategori manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        var catString=catCell.ToString();
                        if (catString==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   Kategori manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        var ingCell=worksheet.Cells[i, header_ingredient].Value;
                        if (ingCell==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   ingrediens manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        var ingString=ingCell.ToString();
                        if (ingString==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   ingrediens manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        var keyCell=worksheet.Cells[i, header_keyword].Value;
                        if (keyCell==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   søgeord manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        var keyString=keyCell.ToString();
                        if (keyString==null)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   søgeord manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        if (ingString.Length==0)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   ingrediens manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        if (keyString.Length==0)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   søgeord manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        if (catString.Length==0)
                        {
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"   kategori manglede i række {i} i "+filePath+" ignorerer linje");
                            continue;
                        }
                        Console.ForegroundColor = OriginalColor;


                        RegexProductPair newPair;
                        
                        try
                        {
                            newPair = new RegexProductPair(catString.Trim(),ingString.Trim(),keyString.Trim());
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine($"  Kunne ikke læse Regex "+keyString+$" på linje {i}, fejlmeddelelse: "+ex.Message+", ignorerer linje");
                            continue;
                        }

                        //Check if ingredient or category need to be replaced

                        //If this is a thing we already have been told to replace, use the other version
                        if (ingredientReplacement.ContainsKey(newPair.ingredient))
                            newPair.ingredient=ingredientReplacement[newPair.ingredient];
                        if (!ingredients.Contains(newPair.ingredient))
                        {
                            bool alreadyExists = false;
                            var synonyms = synonymDictionary.getSimilar(newPair.ingredient);
                            foreach (var synonym in synonyms)
                            {
                                if (ingredients.Contains(synonym))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.Write($"Advarsel:");
                                    Console.ForegroundColor = OriginalColor;
                                    Console.WriteLine("   den automatiske tekstgenkendelse er i tvivl om råvarene "+newPair.ingredient+" og "+synonym+$" er det samme.\nPå linje {i} er produktet \""+newPair.keyword+"\" med råvare \""+newPair.ingredient+"\".\nHvad skal vi gøre?\n  1 erstat råvare "+newPair.ingredient+" med "+synonym+"\n  2 erstat råvare "+synonym+" med "+newPair.ingredient+"\n  3 opret "+newPair.ingredient+" som ny råvare.\nSkriv tal, afslut med Enter");
                                    Console.ForegroundColor = OriginalColor;
                                    int number = 'Q';
                                    while (number!='1' && number!='2' && number!='3')
                                    {
                                        number = Console.Read();
                                    }
                                    if (number=='1')
                                    {
                                        alreadyExists = true;
                                        //This will replace all future
                                        ingredientReplacement.Add(newPair.ingredient,synonym);
                                        newPair.ingredient=synonym;
                                    }
                                    else if (number=='2')
                                    {
                                        //This will replace all future
                                        ingredientReplacement.Add(synonym,newPair.ingredient);
                                        //Also replace all old versions of the old
                                        foreach(var oldPair in ProductPairs)
                                            if (oldPair.ingredient==synonym)
                                                oldPair.ingredient=newPair.ingredient;
                                        ingredients.Remove(synonym);
                                    }
                                    break;
                                }
                            }
                            if (!alreadyExists)
                            {
                                ingredients.Add(newPair.ingredient);
                            }
                        }
                        //If this is a thing we already have been told to replace, use the other version
                        if (categoryReplacement.ContainsKey(newPair.category))
                            newPair.category=categoryReplacement[newPair.category];
                        if (!categories.Contains(newPair.category))
                        {
                            bool alreadyExists = false;
                            var synonyms = synonymDictionary.getSimilar(newPair.category);
                            foreach (var synonym in synonyms)
                            {
                                if (categories.Contains(synonym))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.Write($"Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine("   den automatiske tekstgenkendelse er i tvivl om kategorierne "+newPair.category+" og "+synonym+$" er det samme.\nPå linje {i} er produktet \""+newPair.keyword+"\" med kategori \""+newPair.category+"\".\nHvad skal vi gøre?\n  1 erstat kategori "+newPair.category+" med "+synonym+"\n  2 erstat kategori "+synonym+" med "+newPair.category+"\n  3 opret "+newPair.category+" som ny kategori.\nSkriv tal, afslut med Enter");
                                    Console.ForegroundColor = OriginalColor;
                                    int number = 'Q';
                                    while (number!='1' && number!='2' && number!='3')
                                    {
                                        number = Console.Read();
                                    }
                                    if (number=='1')
                                    {
                                        alreadyExists = true;
                                        //This will replace all future
                                        categoryReplacement.Add(newPair.category,synonym);
                                        newPair.category=synonym;
                                    }
                                    else if (number=='2')
                                    {
                                        //This will replace all future
                                        categoryReplacement.Add(synonym,newPair.category);
                                        //Also replace all old versions of the old
                                        foreach(var oldPair in ProductPairs)
                                            if (oldPair.category==synonym)
                                                oldPair.category=newPair.category;
                                        categories.Remove(synonym);
                                    }
                                    break;
                                }
                            }
                            if (!alreadyExists)
                            {
                                categories.Add(newPair.category);
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        //Because of the compare functions in the pair, this only looks for identical keyword
                        //Just tell the human that it happened, there is no way the program can know which pair is correct.
                        if (ProductPairs.Contains(newPair ))
                        {
                            Console.WriteLine("Advarsel:");
                            Console.ForegroundColor = OriginalColor;
                            Console.WriteLine("   \""+newPair.keyword+"\" duplikeret!, ignorer seneste indgang i "+filePath);
                        }
                        else
                            ProductPairs.Add(newPair);
                        Console.ForegroundColor = OriginalColor;
                    }
                }
            }

            //Now save this to userdata
            string UserdataPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SustainableHospital");
            //Create the directory if it doesn't exist
            Directory.CreateDirectory(UserdataPath);
            string  SavedLibrary = Path.Combine(UserdataPath,"ProduktLibrary.tsv");
            Console.WriteLine("Information, gemmer til "+ SavedLibrary);
            
            //Save to a file so we can easilly load it again
            using (StreamWriter writer = new StreamWriter(SavedLibrary))
            {
                //Write every product pair to the file
                foreach (var pair in ProductPairs)
                    writer.WriteLine($"{pair.category}\t{pair.ingredient}\t{pair.keyword}");
            }

        }

        /// <summary>
        /// Return the best match of the input, best in this case means longest, as that is more specific 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public RegexProductPair? GetMatch(string input)
        {
            foreach (var pair in ProductPairs)
            {
                if (pair.keyRegex.IsMatch(input))
                {
                    
                    //This is the longest, since we have sorted the pairs in order of size
                    return pair;
                }
            }
            return null;
        }
    }
}