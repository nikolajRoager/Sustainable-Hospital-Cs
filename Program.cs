using System.Data;
using OfficeOpenXml;

static class Program
{
    static void Main()
    {
        //For demonstration purpose, my personal NON COMMERCIAL account, this should OBVIOUSLY be replaced!!!!
        ExcelPackage.License.SetNonCommercialPersonal("Nikolaj R Christensen"); 
        
        SynonymDictionary synonyms = new("ddo-synonyms.csv",'\t',';',["..","nogen/noget","(",")"]);
        
        foreach (string s in synonyms.getSimilar("En graf med tid som abscisse"))
        {
            System.Console.WriteLine("Alternative: "+s);
        }

        RegexProductFinder.RegexProductLibrary regexProductLibrary= new RegexProductFinder.RegexProductLibrary("VareTypeBibliotek.xlsx");
        
        
        return;
    }
}