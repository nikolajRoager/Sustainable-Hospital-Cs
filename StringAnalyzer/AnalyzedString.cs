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
    /// Create with 100% chance of being filler
    /// </summary>
    public AnalyzedString()
    {
        filler=1;
        containsTotalMass=0;
        containsSingleMass=0;
        isInteger =0;
        isDecimal= 0;
        this.intValue =0;    
        this.doubleValue=0;
        ProductName=0;
        productNameHeader=0;
        NrHeader=0;
        SingleMassHeader=0;
        TotalMassHeader=0;
        QuantityHeader=0;    

    }

    public int intValue {get;set;}= 0;
    public double doubleValue {get;set;}= 0;

    public AnalyzedString(int intValue)
    {
        filler=0;
        //Maybe... this is one guess as to what this is
        containsTotalMass=1;
        containsSingleMass=1;
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
    }

    public AnalyzedString(double doubleValue)
    {
        filler=0;
        //Maybe... this is one guess as to what this is
        containsTotalMass=1;
        containsSingleMass=1;
        isInteger =10;
        isDecimal= 0;
        this.intValue =(int)doubleValue;    
        this.doubleValue=doubleValue;
        ProductName=0;
        productNameHeader=0;
        NrHeader=0;
        SingleMassHeader=0;
        TotalMassHeader=0;
        QuantityHeader=0;    
    }

}