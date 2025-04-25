using System.Data;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using OfficeOpenXml;

namespace RegexProductFinder
{
    /// <summary>
    /// Library of all products known to the program, can identify the category and igredient of a given keyword
    /// </summary>
    public class RegexProductLibrary
    {
        /// <summary>
        ///The product pairs, sorted by priority, so highest priority keywords come first 
        /// </summary>
        public SortedSet<RegexProductPair> ProductPairs;

        
        /// <summary>
        ///Load library from an Excel file 
        /// </summary>
        public RegexProductLibrary(string filePath)
        {
            if (!File.Exists(filePath))
            {
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
                                //The word ingredient may be included in category
                                if ( CategoryRegex.IsMatch(cellValue.ToString().Trim()))
                                {
                                    header_category=x;
                                }
                                else if (IngredientRegex.IsMatch(cellValue.ToString().Trim()))
                                {
                                    header_ingredient=x;
                                }
                                else if (KeywordRegex.IsMatch(cellValue.ToString().Trim()))
                                {
                                    header_keyword=x;
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
                        throw new ArgumentException("Tabel Header Søgeord, Kategori, og Råvare ikke fundet i "+filePath);

                    ProductPairs = new();
                    for (int i = header_start+1; i < worksheet.Dimension.End.Row; i++)
                    {
                        var newPair = new RegexProductPair(worksheet.Cells[i, header_category].Value.ToString().Trim(),worksheet.Cells[i, header_ingredient].Value.ToString().Trim(),worksheet.Cells[i, header_keyword].Value.ToString().Trim());
                        
                        //Because of the compare functions in the pair, this only looks for identical keyword
                        //Just tell the human that it happened, there is no way the program can know which pair is correct.
                        if (ProductPairs.Contains(newPair ))
                            Console.WriteLine("Advarsel: \""+newPair.keyword+"\" duplikeret!, ignorer seneste indgang i "+filePath);
                        else
                            ProductPairs.Add(newPair);
                    }
                }
            }
        }   
    }
}