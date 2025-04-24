using System.Data;
using System.Reflection.Metadata;
using OfficeOpenXml;

namespace RegexProductFinder
{
    /// <summary>
    /// Library of all products known to the program, can identify the category and igredient of a given keyword
    /// </summary>
    class RegexProductLibrary
    {
        /// <summary>
        ///The product pairs, sorted by priority, so highest priority keywords come first 
        /// </summary>
        //public SortedSet<RegexProductPair> ProductPairs;
        
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
                    
                    //I assume the columns start at 1 1, 1 2, and 1 3
                    for (int i = 2; i < worksheet.Dimension.End.Row; i++)
                    {
                        
                        Console.WriteLine(worksheet.Cells[i, 1].Value + " : " + worksheet.Cells[i, 2].Value + " : " +worksheet.Cells[i, 3].Value );
                        //dataTable.Columns.Add(firstRowCell.Text);
                    }
/*
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var rowCollection = worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column];
                        DataRow newRow = dataTable.Rows.Add();
                        foreach (var cell in rowCollection)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                    }*/
                }
            }
        }   
    }
}