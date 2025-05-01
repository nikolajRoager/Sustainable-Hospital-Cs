
using System.Data;
using System.Text.RegularExpressions;


/// <summary>
/// A list of all products for a single customer
/// </summary>
public class CustomerProductTable
{
    /// <summary>
    /// A single product with a unique name.
    /// For example "Valsemøllen hvedemel" og "budget hvedemel" are both product "hvedemel" but stored separately
    /// </summary>
    public class UniqueCustomerProduct
    {
        double? MassKg { get; set; }
        int? Quantity { get; set;} 
        public String Name { get; set; }
        public string Product { get; set; }
        //Product number, stored as a string because it oft includes other symbols
        public string ProductNumber { get; set; }
        public string Ingredient { get; set; }
        public string Category { get; set; }

        public UniqueCustomerProduct(string name, string productNumber, string product, string ingredient, string category,int? quantity,  double? massKg)
        {
            this.Quantity = quantity;
            this.Name = name;
            this.Product = product;
            this.ProductNumber = productNumber;
            this.Ingredient = ingredient;
            this.Category = category;
            this.MassKg = massKg;
        }
    }
    public static void initializeMatcher()
    {
        //@"\b\d+(?:\.\d+)?.?(?=\s?kg\b)";
    }

    public CustomerProductTable(string ExcelTablePath)
    {
        //First, we must analyze the file, and try to find the columns containing the things we are interested in
    
    }
}