using OfficeOpenXml.Packaging.Ionic.Zip;

namespace DocumentAnalysis
{
    public class hypotheticalTable
    {
        /// <summary>
        /// potential headers, product, number, mass, and amount MAY or MAY NOT coexist the same column can be spread out over two columns for some stupid reason
        /// </summary>
        public List<TableColumnHypothesis> Columns {get; set;}
    }
}