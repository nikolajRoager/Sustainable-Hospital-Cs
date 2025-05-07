using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StringAnalyzer;

namespace DocumentAnalysis
{

    /// <summary>
    /// A class which can analyze a full excel document visually: by simply colour-coding the document
    /// </summary>
    public static class VisualAnalyzer
    {
        /// <summary>
        /// For each sheet in the document, create a new sheet with colour coded cells based on what the cells are individually recognized as
        /// </summary>
        /// <param name="targetDocument"></param>
        static public void vizualizeCellAnalysis(ExcelPackage targetDocument,IStringAnalyzer analyzer)
        {
            int originalSheets = targetDocument.Workbook.Worksheets.Count;
            for (int sheetIndex = 0; sheetIndex < originalSheets; sheetIndex++)
            {
                string OriginalSheetName = targetDocument.Workbook.Worksheets[sheetIndex].Name;
                var newSheet = targetDocument.Workbook.Worksheets.Copy(OriginalSheetName,OriginalSheetName+"-analyzed");
                
                if (newSheet.Dimension==null)//The user knows what file this is
                    throw new Exception($"Empty excel sheet detected at sheet {sheetIndex}");
                //Might be null if empty
                int width = newSheet.Dimension.End.Column;

                //We will gradually build out a legend with the different types of strings
                Dictionary<string,Color> Legend=new();
                
                foreach (var cell in newSheet.Cells)
                {
                    //Get only the various chances of being whatever
                    if (cell!=null)
                    {
                        var type = analyzer.Analyze(cell?.Value?.ToString());
                        
                        string name =
                            $"{(type.filler>0?$"{type.filler*100/type.total}% filler,":"")}"+
                            $"{(type.containsTotalMass>0?$"{type.containsTotalMass*100/type.total}% containsTotalMass,":"")}"+ 
                            $"{(type.containsSingleMass>0?$"{type.containsSingleMass*100/type.total}% containsSingleMass,":"")}"+
                            $"{(type.isInteger>0?$"{type.isInteger*100/type.total}% isInteger,":"")}"+ 
                            $"{(type.isDecimal>0?$"{type.isDecimal*100/type.total}% isDecimal,":"")}"+ 
                            $"{(type.containsProduct>0?$"{type.containsProduct*100/type.total}% containsProduct,":"")}"+ 
                            $"{(type.containsProductNr>0?$"{type.containsProductNr*100/type.total}% containsProductNr,":"")}"+ 
                            $"{(type.containsAmount>0?$"{type.containsAmount*100/type.total}% containsAmount,":"")}"+ 
                            $"{(type.productNameHeader>0?$"{type.productNameHeader*100/type.total}% productNameHeader,":"")}"+  
                            $"{(type.NrHeader>0?$"{type.NrHeader*100/type.total}% NrHeader,":"")}"+ 
                            $"{(type.SingleMassHeader>0?$"{type.SingleMassHeader*100/type.total}% SingleMassHeader,":"")}"+ 
                            $"{(type.TotalMassHeader>0?$"{type.TotalMassHeader*100/type.total}% TotalMassHeader,":"")}"+ 
                            $"{(type.QuantityHeader>0?$"{type.QuantityHeader*100/type.total}% QuantityHeader,":"")}";
                        //Generate the colour
                        int colorR=(
                            Color.Salmon.R *type.filler+
                            Color.DarkRed.R*type.containsTotalMass+
                            Color.Red.R    *type.containsSingleMass+
                            Color.Brown.R  *type.isInteger+
                            Color.Yellow.R *type.isDecimal+
                            Color.Blue.R   *type.containsAmount+
                            Color.Green.R  *type.containsProduct+
                            Color.LightGreen.R*type.productNameHeader+
                            Color.LightSalmon.R*type.NrHeader+
                            Color.LightPink.R*type.SingleMassHeader+
                            Color.Pink.R*type.TotalMassHeader+
                            Color.Cyan.R*type.QuantityHeader)/type.total;
                        int colorG=(
                            Color.Salmon.G *type.filler+
                            Color.DarkRed.R*type.containsTotalMass+
                            Color.Red.R    *type.containsSingleMass+
                            Color.Brown.G  *type.isInteger+
                            Color.Yellow.G *type.isDecimal+
                            Color.Blue.G   *type.containsAmount+
                            Color.Green.G  *type.containsProduct+
                            Color.LightGreen.G*type.productNameHeader+
                            Color.LightSalmon.G*type.NrHeader+
                            Color.LightPink.G*type.SingleMassHeader+
                            Color.Pink.G*type.TotalMassHeader+
                            Color.Cyan.G*type.QuantityHeader)/type.total;
                        int colorB=(
                            Color.Salmon.B *type.filler+
                            Color.DarkRed.B*type.containsTotalMass+
                            Color.Red.R    *type.containsSingleMass+
                            Color.Brown.B  *type.isInteger+
                            Color.Yellow.B *type.isDecimal+
                            Color.Blue.B   *type.containsAmount+
                            Color.Green.B  *type.containsProduct+
                            Color.LightGreen.B*type.productNameHeader+
                            Color.LightSalmon.B*type.NrHeader+
                            Color.LightPink.B*type.SingleMassHeader+
                            Color.Pink.B*type.TotalMassHeader+
                            Color.Cyan.B*type.QuantityHeader)/type.total;

                        if (type.filler!=type.total)
                        {
                            Color col =Color.FromArgb(colorR, colorG, colorB);
                            Legend[name] = col;
                            cell?.Style.Fill.SetBackground(col,ExcelFillStyle.Solid);
                        }
                    }

                    int i =0;
                    foreach (var leg in Legend)
                    {
                        newSheet.Cells[++i,width+1].Value = leg.Key;
                        newSheet.Cells[i,width+1].Style.Fill.SetBackground(leg.Value,ExcelFillStyle.Solid);
                    }
                }


            }
        }
    }
}