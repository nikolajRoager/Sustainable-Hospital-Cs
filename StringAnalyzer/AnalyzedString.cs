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
        public int total {get {return
         filler+  
         containsTotalMass+ 
         containsSingleMass+
         isInteger+ 
         isDecimal+ 
         containsProduct+ 
         containsProductNr+ 
         containsAmount+ 
         productNameHeader+  
         NrHeader+ 
         SingleMassHeader+ 
         TotalMassHeader+ 
         QuantityHeader;
        }}
        //The estimated likelyhood that the string is the following (do note that headers are mutually exclusive with each other and any other content, but product name, and mass can co-exist):
        //The chances go from 0 and up, with higher being more likely
        /// <summary>
        /// chance this is unimportant, mutually exclusive with everything else
        /// </summary>
        public int filler  {get;set;}= 1;

        /// <summary>
        /// The entire string, kept for reference
        /// </summary>
        public string content {get;set;}
        

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
        public int containsProduct {get;set;}= 0;
        /// <summary>
        /// Chance this contains the product number, can coexist with name
        /// </summary>
        public int containsProductNr {get;set;}= 0;
        /// <summary>
        /// Chance this contains the number of instances of this product, can coexist with name
        /// </summary>
        public int containsAmount {get;set;}= 0;
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
        /// If a generic integer has been found, what is it
        /// </summary>
        public int intValue {get;set;}= 0;
        /// <summary>
        /// If a generic double has been found, what is it
        /// </summary>
        public double doubleValue {get;set;}= 0;
        /// <summary>
        /// If product number has been found, what is it
        /// </summary>
        public string ProductNr {get;set;}

        /// <summary>
        /// Create with 100% chance of being filler
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(string c="")
        {
            filler=100;//Filler is always an option
            containsProductNr =0;
            containsTotalMass=0;
            containsSingleMass=0;
            isInteger =0;
            isDecimal= 0;
            containsAmount=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            containsProduct= 0;
            Product = null;
            intValue =0;    
            doubleValue=0;
            content=c;
            ProductNr="null";
        }


        /// <summary>
        /// Create with 100% chance of being a product
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(IProduct P,string c)
        {
            filler=1;//Filler is always an option
            containsTotalMass=0;
            containsSingleMass=0;
            isInteger =0;
            isDecimal= 0;
            intValue =0;    
            doubleValue=0;
            containsAmount=0;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            Product = P;
            containsProductNr =0;
            containsProduct= 10;
            content = c;
            ProductNr="null";
        }


        /// <summary>
        /// Create from an generic integer value
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(int intValue, string c)
        {
            filler=1;//Filler is always an option
            //Maybe one of these, idk.
            containsTotalMass=2;
            containsSingleMass=2;
            containsAmount=2;
            containsProductNr =2;
            isInteger =10;
            isDecimal= 0;
            this.intValue =intValue;    
            this.doubleValue=intValue;
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;    
            Product = null;
            containsProduct= 0;
            content = c;
            ProductNr=$"{intValue}";
        }

        /// <summary>
        /// Create from an generic double value
        /// Normally I recommend using custom constructors with the new keyword, but there are predefined constructors for the most common things
        /// </summary>
        public AnalyzedString(double doubleValue, string c)
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
            productNameHeader=0;
            NrHeader=0;
            SingleMassHeader=0;
            TotalMassHeader=0;
            QuantityHeader=0;
            Product = null;
            containsProduct= 0;
            content = c;
            ProductNr=$"{intValue}";
        }

        /// <summary>
        /// Create a clone without the estimated value of the int, product number, double, or product in this string
        /// Useful if we want to group up similar cells, regardless of content
        /// </summary>
        /// <returns></returns>
        public AnalyzedString cloneChances()
        {
            return new AnalyzedString{
                filler=this.filler,
                containsTotalMass=this.containsTotalMass,
                containsSingleMass=this.containsSingleMass,
                containsAmount=this.containsAmount,
                isInteger=this.isInteger,
                isDecimal=this.isDecimal,
                containsProductNr=this.containsProductNr,
                intValue=0,
                doubleValue=0,
                NrHeader=this.NrHeader,
                SingleMassHeader=this.SingleMassHeader,
                TotalMassHeader=this.TotalMassHeader,
                QuantityHeader=this.QuantityHeader,
                Product=null,
                containsProduct=this.containsProduct,
                content="",
                ProductNr="null"
            };
        }
    }
}