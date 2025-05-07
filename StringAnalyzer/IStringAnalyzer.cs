using System.Runtime.InteropServices.Marshalling;
using OfficeOpenXml;
using Services;
using UserInterface;

namespace StringAnalyzer
{
    /// <summary>
    /// An interface for a string analyzer: the class which looks at a single string, and makes an educated guess what it is we are looking at
    /// The string analyzer doesn't know the larger context
    /// </summary>
    public interface IStringAnalyzer
    {
        /// <summary>
        /// Re-train the analyzer, so it can work on different products or different languages, it will ask the user for additional information and files
        /// </summary>
        public void train(IFileWriter PersistFile, ExcelPackage library, IUI UserInterface);

        public void load(IFileReader PersistReader);

        /// <summary>
        /// Run the analysis on the input, and spit out what we think it might be
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public AnalyzedString Analyze(string? input);

    }
}