using System.Globalization;
using StringAnalyzer;

namespace DocumentAnalysis
{
    /// <summary>
    /// The result of the first pass, where we only analyze the individual cells
    /// </summary>
    public class FirstAnalysisDocument
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// The individual cells
        /// </summary>
        public AnalyzedString[,] Cells {get; set;}

        public FirstAnalysisDocument(string Name,int Height, int Width)
        {
            this.Name = Name;
            this.Width = Width;
            this.Height = Height;
            Cells = new AnalyzedString[Height,Width];
        } 
    }
}