using System.Net.Http.Headers;

namespace StringAnalyzer
{
    /// <summary>
    /// The result of the analysis: and estimate of what this could be with likelyhoods of being different things
    /// The likelyhood is an arbitrary integer, with higher numbers being more likely and 0 meaning that it is certainly not that thing
    /// Some things are mutually exclusive (like headers and products), while other things can co-exist (like unit mass and product)
    /// </summary>
    public class AnalyzedString
    {
        //The estimated likelyhood that the string is the following (do note that headers are mutually exclusive with each other and any other content, but product name, and mass can co-exist):
        //The chances go from 0 and up, with higher being more likely
        /// <summary>
        /// chance this is unimportant, mutually exclusive with everything else
        /// </summary>
        public int filler  {get;set;}= 1;
        /// <summary>
        /// Chance this contains the total mass 
        /// </summary>
        public int containsTotalMass {get;set;}= 0;
        /// <summary>
        /// Chance this contains mass of a single unit
        /// </summary>
        public int containsSingleMass {get;set;}= 0;
        /// <summary>
        /// Chance this is just plainly an integer , mutually exclusive with everything
        /// </summary>
        public int isInteger {get;set;}= 0;
        /// <summary>
        /// Chance this is just plainly a decimal, mutually exclusive with everything
        /// </summary>
        public int isDecimal {get;set;}= 0;
        /// <summary>
        /// Chance this is a product name, can coexist with some numbers like mass or amount
        /// </summary>
        public int isProduct {get;set;}= 0;
        /// <summary>
        /// Chance this contains the product number, can coexist with name
        /// </summary>
        public int containsProductNr {get;set;}= 0;
        /// <summary>
        /// Chance this contains the number of instances of this product, can coexist with name
        /// </summary>
        public int containsAmount {get;set;}= 0;
        /// <summary>
        /// Chance this is the name of a product, can coexist with mass
        /// </summary>
        public int ProductName  {get;set;}= 0;
        /// <summary>
        /// Chance this is the header for the product name
        /// </summary>
        public int productNameHeader  {get;set;}=0;
        /// <summary>
        /// Chance this is the header for the product number
        /// </summary>
        public int NrHeader  {get;set;}=0;
        /// <summary>
        /// Chance this is a header for the mass of a single item
        /// </summary>
        public int SingleMassHeader {get;set;}=0;
        /// <summary>
        /// Chance this is a header for the total mass of a all items
        /// </summary>
        public int TotalMassHeader {get;set;}=0;
        /// <summary>
        /// Chance this is a header for the number of items
        /// </summary>
        public int QuantityHeader {get;set;}=0;

        /// <summary>
        /// If this is a product, what do we think it is?
        /// </summary>
        public IProduct? Product {get;set;}= null;

        /// <summary>
        /// Create with 100% chance of being filler
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString()
        {
            filler=100;//Filler is always an option
            containsProductNr =0;
            containsTotalMass=0;
            containsSingleMass=0;
            isInteger =0;
            isDecimal= 0;
            intValue =0;    
            doubleValue=0;
            ProductName=0;
            containsAmount=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            Product = null;
            isProduct= 0;
        }

        public int intValue {get;set;}= 0;
        public double doubleValue {get;set;}= 0;


        /// <summary>
        /// Create with 100% chance of being a product
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(IProduct P)
        {
            filler=1;//Filler is always an option
            containsTotalMass=0;
            containsSingleMass=0;
            isInteger =0;
            isDecimal= 0;
            intValue =0;    
            doubleValue=0;
            ProductName=0;
            containsAmount=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            Product = P;
            containsProductNr =0;
            isProduct= 10;
        }


        /// <summary>
        /// Create from an generic integer value
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(int intValue)
        {
            filler=1;//Filler is always an option
            //Maybe one of these, idk.
            containsTotalMass=2;
            containsSingleMass=2;
            containsAmount=2;
            containsProductNr =1;
            isInteger =10;
            isDecimal= 0;
            this.intValue =intValue;    
            this.doubleValue=intValue;
            ProductName=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            Product = null;
            isProduct= 0;
        }

        /// <summary>
        /// Create from an generic double value
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(double doubleValue)
        {
            filler=1;//Filler is always an option
            //Maybe... mass, certainly not amount, unless it is actually an int
            containsTotalMass=5;
            containsSingleMass=5;
            containsAmount=(doubleValue==(int)(doubleValue) ? 2 :0);//The fact we got sent it as a double, means it had a period, so i doubt it is an int
            isInteger =(doubleValue==(int)(doubleValue) ? 3 :0);//The fact we got sent it as a double, means it had a period, so i doubt it is an int
            isDecimal= 10;
            containsProductNr =(doubleValue==(int)(doubleValue) ? 1 :0);//I highly doubt this is product nr BUT IT COULD BE
            this.intValue =(int)doubleValue;    
            this.doubleValue=doubleValue;
            ProductName=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;
            Product = null;
            isProduct= 0;
        }
    }
}