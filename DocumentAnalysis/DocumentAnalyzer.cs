using System.Drawing;
using System.Net.Http.Headers;
using System.Security.Cryptography.Pkcs;
using OfficeOpenXml;
using StringAnalyzer;
using UserInterface;

namespace DocumentAnalysis
{
    /// <summary>
    /// The glorious class which fully analyzes an Excel document ... by calling other classes, and passing their work of as our own (the other classes are defined by interfaces, thus making it highly flexible)
    /// </summary>
    class DocumentAnalyzer(IStringAnalyzer _stringAnalyzer, IUI uI)
    {
        private IUI UI = uI;
        private IStringAnalyzer stringAnalyzer = _stringAnalyzer;

        /// <summary>
        /// Perform first pass on this Excel document, analyzing each cell in isolation, as there may be multiple sheets we return result for each sheet
        /// Optionally change the colours in the target document to reflect the analysis
        /// </summary>
        /// <param name="targetDocument"></param>
        /// <param name="visual">If this parameter is true, we change the colours in the target document to reflect our analysis</param>
        /// <returns></returns>
        public List<FirstAnalysisDocument> firstPass(ExcelPackage targetDocument,bool visual=false)
        {
            int originalSheets = targetDocument.Workbook.Worksheets.Count;
            List<FirstAnalysisDocument> Out = new();
            for (int sheetIndex = 0; sheetIndex < originalSheets; sheetIndex++)
            {
                
                var newSheet = targetDocument.Workbook.Worksheets.Copy(targetDocument.Workbook.Worksheets[sheetIndex].Name,"FirstPass"+(originalSheets>1?$"{sheetIndex}":"")) ;
                
                if (newSheet.Dimension==null)//The user knows what file this is
                {
                    UI.WriteLine($"tomt ark: {sheetIndex}",true);
                    continue;
                }
                //Might be null if empty
                int width = newSheet.Dimension.End.Column;
                int height = newSheet.Dimension.End.Row;

                FirstAnalysisDocument ThisOut = new FirstAnalysisDocument(targetDocument.Workbook.Worksheets[sheetIndex].Name,height,width);

                //We will gradually build out a legend with the different types of strings
                Dictionary<string,Color> Legend=new();

                for (int y = 0; y < height; ++y)
                {
                    if (y % 100 == 0)
                        Console.WriteLine($"{y}/{height}");
                for (int x = 0; x < width; ++x)
                        {
                            var cell = newSheet.Cells[y + 1, x + 1];
                            //Get only the various chances of being whatever
                            if (cell != null)
                            {
                                var type = stringAnalyzer.Analyze(cell?.Value?.ToString());
                                ThisOut.Cells[y, x] = type;

                                //If need be, generate a unique name and colour for thsi combination
                                if (visual)
                                {
                                    string name =
                                        $"{(type.filler > 0 ? $"{type.filler * 100 / type.total}% filler," : "")}" +
                                        $"{(type.containsTotalMass > 0 ? $"{type.containsTotalMass * 100 / type.total}% containsTotalMass," : "")}" +
                                        $"{(type.containsSingleMass > 0 ? $"{type.containsSingleMass * 100 / type.total}% containsSingleMass," : "")}" +
                                        $"{(type.isInteger > 0 ? $"{type.isInteger * 100 / type.total}% isInteger," : "")}" +
                                        $"{(type.isDecimal > 0 ? $"{type.isDecimal * 100 / type.total}% isDecimal," : "")}" +
                                        $"{(type.containsProduct > 0 ? $"{type.containsProduct * 100 / type.total}% containsProduct," : "")}" +
                                        $"{(type.containsProductNr > 0 ? $"{type.containsProductNr * 100 / type.total}% containsProductNr," : "")}" +
                                        $"{(type.containsAmount > 0 ? $"{type.containsAmount * 100 / type.total}% containsAmount," : "")}" +
                                        $"{(type.productNameHeader > 0 ? $"{type.productNameHeader * 100 / type.total}% productNameHeader," : "")}" +
                                        $"{(type.NrHeader > 0 ? $"{type.NrHeader * 100 / type.total}% NrHeader," : "")}" +
                                        $"{(type.SingleMassHeader > 0 ? $"{type.SingleMassHeader * 100 / type.total}% SingleMassHeader," : "")}" +
                                        $"{(type.TotalMassHeader > 0 ? $"{type.TotalMassHeader * 100 / type.total}% TotalMassHeader," : "")}" +
                                        $"{(type.QuantityHeader > 0 ? $"{type.QuantityHeader * 100 / type.total}% QuantityHeader," : "")}";
                                    //Generate the colour
                                    int colorR = (
                                        Color.Salmon.R * type.filler +
                                        Color.DarkRed.R * type.containsTotalMass +
                                        Color.Red.R * type.containsSingleMass +
                                        Color.Brown.R * type.isInteger +
                                        Color.Yellow.R * type.isDecimal +
                                        Color.Blue.R * type.containsAmount +
                                        Color.Green.R * type.containsProduct +
                                        Color.LightGreen.R * type.productNameHeader +
                                        Color.LightSalmon.R * type.NrHeader +
                                        Color.LightPink.R * type.SingleMassHeader +
                                        Color.Pink.R * type.TotalMassHeader +
                                        Color.Cyan.R * type.QuantityHeader) / type.total;
                                    int colorG = (
                                        Color.Salmon.G * type.filler +
                                        Color.DarkRed.R * type.containsTotalMass +
                                        Color.Red.R * type.containsSingleMass +
                                        Color.Brown.G * type.isInteger +
                                        Color.Yellow.G * type.isDecimal +
                                        Color.Blue.G * type.containsAmount +
                                        Color.Green.G * type.containsProduct +
                                        Color.LightGreen.G * type.productNameHeader +
                                        Color.LightSalmon.G * type.NrHeader +
                                        Color.LightPink.G * type.SingleMassHeader +
                                        Color.Pink.G * type.TotalMassHeader +
                                        Color.Cyan.G * type.QuantityHeader) / type.total;
                                    int colorB = (
                                        Color.Salmon.B * type.filler +
                                        Color.DarkRed.B * type.containsTotalMass +
                                        Color.Red.R * type.containsSingleMass +
                                        Color.Brown.B * type.isInteger +
                                        Color.Yellow.B * type.isDecimal +
                                        Color.Blue.B * type.containsAmount +
                                        Color.Green.B * type.containsProduct +
                                        Color.LightGreen.B * type.productNameHeader +
                                        Color.LightSalmon.B * type.NrHeader +
                                        Color.LightPink.B * type.SingleMassHeader +
                                        Color.Pink.B * type.TotalMassHeader +
                                        Color.Cyan.B * type.QuantityHeader) / type.total;

                                    if (type.filler != type.total)
                                    {
                                        Color col = Color.FromArgb(colorR, colorG, colorB);
                                        Legend[name] = col;
                                        cell?.Style.Fill.SetBackground(col, OfficeOpenXml.Style.ExcelFillStyle.Solid);
                                    }
                                }
                            }
                        }

                    //If need be, add description of what the colours mean
                    if (visual)
                    {
                        int i =0;
                        foreach (var leg in Legend)
                        {
                            newSheet.Cells[++i,width+1].Value = leg.Key;
                            newSheet.Cells[i,width+1].Style.Fill.SetBackground(leg.Value,OfficeOpenXml.Style.ExcelFillStyle.Solid);
                        }

                    }
                }
                Out.Add(ThisOut);
            }
            return Out;
        }

        /// <summary>
        /// Make both first and second pass, and optionally display what happened
        /// </summary>
        /// <param name="package"></param>
        /// <param name="visual"></param>
        public void Analyze(ExcelPackage package, bool visual=false,bool noAdd=false)
        {
            UI.WriteLine("Første analyse");
            var documents = firstPass(package,visual);
            UI.WriteLine("Anden analyse");
            for (int i = 0; i < documents.Count; ++i)
            {
                UI.WriteLine($"{i+1}/{documents.Count}");
                var tables = secondPass(documents[i],noAdd);

                foreach (var table in tables)
                    thirdPass(documents[i],table);

                if (visual)
                {
                    //copy the original sheet
                    var newSheet = package.Workbook.Worksheets.Copy(documents[i].Name,"SecondPass"+(documents.Count>1?$"{i}":"")) ;
                    
                    foreach (var table in tables)
                    {
                        for (int x = table.x0; x <=table.x1; ++x)
                            newSheet.Cells[table.y0+1,x+1].Style.Fill.SetBackground(Color.Red,OfficeOpenXml.Style.ExcelFillStyle.Solid);
                        foreach (var col in table.Columns)
                        {
                            newSheet.Cells[table.y0+1,col.column_x+1].Style.Fill.SetBackground(Color.DarkGreen,OfficeOpenXml.Style.ExcelFillStyle.Solid);

                            //Update header to say what we think it is
                            newSheet.Cells[table.y0+1,col.column_x+1].Value =
                                $"{(col.couldBeProduct ? $"PRODUKT {table.Columns.Count(c=>c.couldBeProduct)}, ":"" )}{(col.couldBeNumber? $"VARENR  {table.Columns.Count(c=>c.couldBeNumber)}, ":"" )}{(col.couldBeAmount? $"ANTAL {table.Columns.Count(c=>c.couldBeAmount)}, ":"" )}{(col.couldBeSingleMass? $"STK. MASSE {table.Columns.Count(c=>c.couldBeSingleMass)}, ":"" )}{(col.couldBeTotalMass? $"TOTAL MASSE {table.Columns.Count(c=>c.couldBeTotalMass)}":"" )}";

                            for (int y = table.y0+1; y <table.y1; ++y)
                            {
                                newSheet.Cells[y+1,col.column_x+1].Style.Fill.SetBackground(col.column_x%2==0 ? Color.Green : Color.SeaGreen,OfficeOpenXml.Style.ExcelFillStyle.Solid);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<hypotheticalTable> secondPass(FirstAnalysisDocument Document,bool noAdd=false)
        {

            HashSet<string> nonProducts=new();//Things we already warned the user is not a product, no need to warn them again

            //There may be several tables in the same document
            Dictionary<(int,int),hypotheticalTable> tables = new ();
            //First step, loop through all rows, and see if it contains headers
            for (int y = 0; y < Document.Height; ++y)
            {
                hypotheticalTable thisTable = new(y);
                for (int x = 0; x < Document.Width; ++x)
                {
                    //Skip things which could be a product
                    if     (Document.Cells[y,x].containsProduct==0 &&
                          ((Document.Cells[y,x].NrHeader>0)
                        || (Document.Cells[y,x].QuantityHeader>0)
                        || (Document.Cells[y,x].TotalMassHeader>0)
                        || (Document.Cells[y,x].SingleMassHeader>0)
                        ||  Document.Cells[y,x].productNameHeader>0))
                    {
                        //product names can contain a lot of information
                        thisTable.Add(new HypotheticalColumn{
                            couldBeNumber=Document.Cells[y,x].NrHeader>0 || Document.Cells[y,x].productNameHeader>0,
                            couldBeAmount=Document.Cells[y,x].QuantityHeader>0 || Document.Cells[y,x].productNameHeader>0,
                            couldBeTotalMass=Document.Cells[y,x].TotalMassHeader>0 || Document.Cells[y,x].productNameHeader>0,
                            couldBeSingleMass=Document.Cells[y,x].SingleMassHeader>0 || Document.Cells[y,x].productNameHeader>0,
                            couldBeProduct=Document.Cells[y,x].productNameHeader>0,
                            column_x=x});
                    }
                }

                //Only add if we detected all headers
                if (thisTable.headers==4)
                   tables[(y,thisTable.x0)]=(thisTable);
            }
            if (tables.Count==0)
            {
                UI.WriteLine($"Ingen tabel fundet i dokument",true);
                return new List<hypotheticalTable>();
            }

            //Ok, now step down through the table to see how deep it goes ... and fix any ambiguity
            //Loop from below instead, that way already finished tables can block the ones we are working on
            foreach (((int y0,int x0),hypotheticalTable table) in tables)
            {
                //Extend the size of the table as we go
                for (table.y1 = y0+1; table.y1 < Document.Height; ++table.y1)
                {

                    //Check for overlap with other tables below this

                    foreach (((int y02,int x02),hypotheticalTable other) in tables)
                    if (table.y1>=other.y0 && table.y1<other.y1)
                    {
                      //These are all the ways there can be overlap, if there is break!!!
                        if ( (table.x0<other.x0 && table.x1>other.x0) ||  (table.x0<other.x1 && table.x1>other.x1)  ||  (other.x0<table.x0 && other.x1>table.x0)   ||  (other.x0<table.x1 && other.x1>table.x1) )
                        {
                           break;
                        }
                    }

                    //Check if ALL suddenly stopped at the same time, in that case we simply stop
                    bool anyMatches=false;
                    foreach (HypotheticalColumn col in table.Columns)
                    {
                        if (col.couldBeNumber && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsProductNr>0)
                        {
                            anyMatches=true;
                        }
                        if (col.couldBeProduct && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsProduct>0)
                        {
                            anyMatches=true;
                        }
                        if (col.couldBeAmount && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsAmount>0)
                        {
                            anyMatches=true;
                        }
                        if ((col.couldBeTotalMass || col.couldBeSingleMass) && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && (Document.Cells[table.y1,col.column_x].containsSingleMass>0 || Document.Cells[table.y1,col.column_x].containsTotalMass>0))
                        {
                            anyMatches=true;
                        }
                    }
                    //If this is a non-matching but not blank line, end the table
                    //Tables can continue after blank lines
                    if (!anyMatches)
                        break;

                    //Has our search had to end, because one important column stopped?
                    bool endSearch= false;
                    //Otherwise, non-blank mismatches disable columns
                    foreach (HypotheticalColumn col in table.Columns)
                    {
                        //Re-analyze if something has changed, this makes it slower but is needed
                        if (stringAnalyzer.isModified)
                            Document.Cells[table.y1,col.column_x]=stringAnalyzer.Analyze(Document.Cells[table.y1,col.column_x].content);
                        if (col.couldBeNumber && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsProductNr==0)
                        {
                            //Ok, either this is not the number column OR we should stop now, if this column is the last remaining number column we have to stop
                            if (table.Columns.Count(c=>c.couldBeNumber)>1)
                                col.couldBeNumber=false;
                            else {
                                //Product number is not STRICTLY required
                                /*endSearch=true;break;*/
                                col.couldBeNumber=false;
                                }
                        }
                        if (col.couldBeProduct && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsProduct==0)
                        {
                            //In THIS case with products, we should do a more detailed fuzzy and synonym text search, to catch mis-spellings or synonyms, this is not done in every case because it is SLOOOOW
                            Document.Cells[table.y1,col.column_x]=stringAnalyzer.AnalyzeDetailed(Document.Cells[table.y1,col.column_x].content);

                            //Try again
                            if (col.couldBeProduct && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsProduct==0)
                            {
                                //Only warn the user if this is the first occurance, and if we are allowed to bother them
                                int userOptionAddNewProduct=0;
                                if (!nonProducts.Contains(Document.Cells[table.y1,col.column_x].content) && !noAdd)
                                {
                                    UI.WriteLine($"På linje {table.y1+1} søjle {col.column_x+1} vurdere vi at "+Document.Cells[table.y1,col.column_x].content+" ikke er et produkt; hvis du er uenig skal det tilføjes til træningsdata og programmet bør gentrænes! Hvis ikke kan du ignorere det",true);
                                    userOptionAddNewProduct = UI.selectOption($"Hvad skal vi gøre?",["Ikke et produkt!","Tilføj nyt produkt"]);
                                }
                                if (userOptionAddNewProduct==1 && stringAnalyzer.retrainWord(Document.Cells[table.y1,col.column_x].content))
                                {
                                    //The user fixed the error, now this IS a product, go on then
                                }
                                else
                                {
                                    nonProducts.Add(Document.Cells[table.y1,col.column_x].content);

                                    //Same same
                                    if (table.Columns.Count(c=>c.couldBeProduct)>1)
                                    {
                                        col.couldBeProduct=false;
                                    }
                                    else
                                    {
                                        {
                                            endSearch=true;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                        if (col.couldBeAmount && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsAmount==0)
                        {
                            //Same same
                            if (table.Columns.Count(c=>c.couldBeAmount)>1)
                                col.couldBeAmount=false;
                            else {endSearch=true;break;}
                        }
                        if ((col.couldBeTotalMass || col.couldBeSingleMass) && !String.IsNullOrEmpty(Document.Cells[table.y1,col.column_x].content) && Document.Cells[table.y1,col.column_x].containsSingleMass==0 && Document.Cells[table.y1,col.column_x].containsTotalMass==0)
                        {
                            //Same same
                            if (table.Columns.Count(c=>c.couldBeSingleMass || c.couldBeTotalMass)>1)
                            {
                                col.couldBeTotalMass=false;
                                col.couldBeSingleMass=false;
                            }
                            else endSearch=true;
                        }
                    }
                    table.Columns.RemoveWhere(c=>c.ambiguoity==0);//Ambiguity 0 means there is literally nothing it could be
                    if (endSearch)
                        break;
                }

                //Now drop any tables which are obviously incomplete
                if (table.missingEssential || table.y0+1==table.y1)
                {
                    tables.Remove((y0,x0));
                }
            }

            return tables.Values;
        }

        /// <summary>
        /// Generate all legal possibilities given the rule that:
        /// only 1 product column, if there are multiple they must be empty when the other column is not
        /// Sum of unit mass times amount must be total mass; there may be up to 3 columns with unit mass, these may be dublicate or sum up to total mass
        /// Total mass and amount must be exactly once, product number may be 0 to 2 times
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public hypotheticalTable? thirdPass(FirstAnalysisDocument analyzedStrings, hypotheticalTable table)
        {
            //First generate all possible tables of columns, using the binary signature
            List<List<HypotheticalColumn>> columnOptions = new();
            foreach (HypotheticalColumn col in table.Columns)
            {
                List<HypotheticalColumn> ThisColumnOptions = new();
                //Create all columns based on the signature, allowed signatures are these:
                int[] legalSignatures =
                {
                    0b10000,
                    0b11000,
                    0b10100,
                    0b11100,
                    0b10010,
                    0b11010,
                    0b10110,
                    0b11110,
                    0b10001,
                    0b11001,
                    0b10101,
                    0b11101,
                    0b01000,
                    0b00100,
                    0b00010,
                    0b00001
                };
                //Include the optioin that this column is nothing
                ThisColumnOptions.Add(
        new HypotheticalColumn
                        {
                            column_x = col.column_x,
                            header_y = col.header_y,
                            signature = 0,
                        }
                );

                Console.WriteLine($"y {col.header_y} x {col.column_x} product {col.couldBeProduct} number {col.couldBeNumber} amount {col.couldBeAmount} single {col.couldBeSingleMass} total {col.couldBeTotalMass}");
                
                foreach (int sig in legalSignatures)
                {
                    //This means this legal signature is legal
                    if ((sig & col.signature) == sig)
                    {
                        //This is the possible column this could be
                        HypotheticalColumn newCol = new HypotheticalColumn
                        {
                            column_x = col.column_x,
                            header_y = col.header_y,
                            signature = sig,
                        };
                        //Add this, if the signature isn't already there
                        if (ThisColumnOptions.Count(c => c.signature == sig) == 0)
                            ThisColumnOptions.Add(newCol);
                    }
                }
                columnOptions.Add(ThisColumnOptions);
            }

            //Now loop through each and every option, these indices are where in the looping over each column we are
            List<int> indices = new List<int>();
            for (int i = 0; i < table.Columns.Count; ++i)
                indices.Add(0);

            while (indices[table.Columns.Count-1] < columnOptions[table.Columns.Count-1].Count())
            {

                bool reject = false;
                int numberCol= -1;//-1 = not found
                int amountCol= -1;
                int productCol= -1;
                int nMassColFound = 0;
                for (int i = 0; i < indices.Count; ++i)
                {
                    var col = columnOptions[i][indices[i]];
                    if (col.couldBeNumber)
                    {
                        //Only one column with product number allowed
                        if (numberCol != -1)
                        {
                            reject = true;
                            break;
                        }
                        numberCol = i;
                    }
                    //Only one product
                    if (col.couldBeProduct)
                    {
                        if (productCol != -1)
                        {
                            reject = true;
                            break;
                        }
                        productCol = i;
                    }
                    //Only one product
                    if (col.couldBeAmount)
                    {
                        if (amountCol != -1)
                        {
                            reject = true;
                            break;
                        }
                        amountCol = i;
                    }
                    if (col.couldBeTotalMass || col.couldBeSingleMass)
                    {
                        ++nMassColFound;
                    }

                }
                if (reject || amountCol == -1 || productCol == -1 || numberCol == -1 || nMassColFound==0)
                {/*This is not a legal option, ignore*/}
                else
                {

                    //Now check the legality of mass, single mass must be = total mass for every row
                    //Also verify that product numbers are unique to prodducts
                    Dictionary<string,IProduct> productNumberPairs = new();
                    for (int y = table.y0+1; y < table.y1; y++)
                    {
                        string productNr = analyzedStrings.Cells[y, columnOptions[numberCol][indices[numberCol]].column_x].content; 
                        IProduct? product = analyzedStrings.Cells[y, columnOptions[productCol][indices[productCol]].column_x].Product;
                        if (product == null || (productNumberPairs.ContainsKey(productNr) && productNumberPairs[productNr] != product))
                        {
                            reject = true;
                            break;
                        }
                        else
                        {
                            productNumberPairs[productNr] = product;

                        }

                        double totalMass = 0;
                        double singleMass = 0;
                        double amount = 0;
                        for (int i = 0; i < indices.Count; ++i)
                        {
                            var col = columnOptions[i][indices[i]];

                            if (col.couldBeAmount)
                            {
                                amount += analyzedStrings.Cells[y, col.column_x].doubleValue;
                            }
                            else if (col.couldBeSingleMass)
                                singleMass += analyzedStrings.Cells[y, col.column_x].massValue;
                            else if (col.couldBeTotalMass)
                                totalMass += analyzedStrings.Cells[y, col.column_x].massValue;
                        }
                        //The exact match is not a bug, we are checking if single or total mass has never been modified (it didn't exist in the dataset)
                        if (singleMass == 0 && totalMass != 0)
                        {
                            //Calculate the single mass from the amount
                            singleMass = totalMass / amount;

                        }
                        else if (singleMass == 0 && totalMass != 0)
                        {
                            totalMass = singleMass * amount;
                        }
                        else if (!(singleMass * amount > totalMass - 0.1 && singleMass * amount < totalMass + 0.1))
                            {
                                reject = true;
                                break;
                            }
                    }


                    if (!reject)
                        {
                            for (int i = 0; i < indices.Count; ++i)
                            {
                                var col = columnOptions[i][indices[i]];
                                Console.Write($"{i}: y {col.header_y} x {col.column_x}: ");
                                if (col.couldBeProduct)
                                    Console.WriteLine("Product");
                                else if (col.couldBeNumber)
                                    Console.WriteLine("Number");
                                else if (col.couldBeAmount)
                                    Console.WriteLine("Amount");
                                else if (col.couldBeSingleMass)
                                    Console.WriteLine("Single Mass");
                                else if (col.couldBeTotalMass)
                                    Console.WriteLine("Total Mass");
                                else
                                    Console.WriteLine("None");
                            }
                        }

                }
                //Increment the index and wrap around
                ++indices[0];
                for (int i = 0; i+1 < indices.Count; ++i)
                {
                    if (indices[i] >= columnOptions[i].Count)
                    {
                        indices[i] = 0;
                        indices[i + 1]++;
                    }
                }
            }
            return null;
        }
    }
}