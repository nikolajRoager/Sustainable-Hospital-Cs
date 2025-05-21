using OfficeOpenXml.Packaging.Ionic.Zip;

namespace DocumentAnalysis
{
    /*package private*/ class hypotheticalTable
    {
        /// <summary>
        /// potential headers, product, number, mass, and amount MAY or MAY NOT coexist the same column can be spread out over two columns for some stupid reason
        /// </summary>
        public HashSet<HypotheticalColumn> Columns {get; set;}
        public int y0 {get;}
        public int y1 {get;set;}

        public hypotheticalTable(int y)
        {
            this.y0 = y;
            Columns = new ();
        }

    /// <summary>
    /// How many of the important headers do we have? in this case I count mass as 1 because with amount and total or single mass it is trivial to find
    /// Ambiguity may make this number drop with further analysis
    /// </summary>
        public int headers
        {
          get {
            return (Columns.Count(x=>x.couldBeProduct)>0?1:0) + (Columns.Count(x=>x.couldBeAmount)>0?1:0) + (Columns.Count(x=>x.couldBeNumber)>0?1:0) + (Columns.Count(x=> (x.couldBeSingleMass || x.couldBeTotalMass ))>0?1:0);
          }  
        }

        public int x0
        {
            get
            {
                return Columns.Min(c=>c.column_x);
            }
        }
        public int x1
        {
            get
            {
                return Columns.Max(c=>c.column_x);
            }
        }

        //Is anything missing from the columns!?
        public bool missingAny
        {
            get
            {
                return (Columns.Count(c=>c.couldBeAmount)==0) ||(Columns.Count(c=>c.couldBeProduct)==0) ||(Columns.Count(c=>c.couldBeNumber)==0) || ((Columns.Count(c=>c.couldBeSingleMass)==0) && (Columns.Count(c=>c.couldBeTotalMass)==0));
            }
        }

        /// <summary>
        /// Is anything IMPORTANT missing from the columns!?
        /// Returns true if we do not have amount or masses (which we can o deduce amount from) and product
        /// </summary>
        public bool missingEssential
        {
            get
            {
                return Columns.Count(c=>c.couldBeProduct)==0;
            }
        }


        /// <summary>
        /// Add a potential column to this potential table, the table takes care of registering it correctly
        /// </summary>
        /// <param name="column"></param>
        public void Add(HypotheticalColumn column)
        {
            column.header_y = y0;
            Columns.Add(column);
        }
    }
}