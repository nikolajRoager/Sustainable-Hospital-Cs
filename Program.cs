using System.Data;
using OfficeOpenXml;

static class Program
{
    static void Main()
    {
        //For demonstration purpose, my personal NON COMMERCIAL account, this should OBVIOUSLY be replaced!!!!
        ExcelPackage.License.SetNonCommercialPersonal("Nikolaj R Christensen"); 
        RegexProductFinder.RegexProductLibrary regexProductLibrary= new RegexProductFinder.RegexProductLibrary("VareTypeBibliotek.xlsx");
        return;
    }
}