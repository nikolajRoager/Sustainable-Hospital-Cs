/// <summary>
/// Something which can be in a string
/// </summary>
enum StringContentType
{
    // No idea what this is
    GenericText = 0,
    // This is the name of a product, may include more information
    ProductName = 1,
    // This is an integer number
    Integer = 2,
    // This is a double number
    Double = 3,
    // This is a header for the product name
    productNameHeader =5,
    // This is a header for the product number
    NrHeader =6,
    // This is a header for the mass of a single item
    SingleMassHeader=7,
    // This is a header for the total mass of a all items
    TotalMassHeader=8,
    // This is a header for the number of items
    QuantityHeader=9,

}

/// <summary>
/// Some part of a string
/// </summary>
class StringContent
{
    /// <summary>
    /// What do we think this is?
    /// </summary>
    public StringContent type { get; set; }
    /// <summary>
    /// On a scale from 0 to 10, How certain are we this is what we think it is?
    /// higher number means more certain, 0 means "highly unlikely", 10 means "very certain"
    /// </summary>
    public int certainty { get; set; }


    /// <summary>
    /// Where in the string is this piece of content located?, and how long is it
    /// </summary>
    int start, length;

    /// <summary>
    /// If relevant what is the string, integer or decimal value of this thing
    /// </summary>
    public int? integerValue {get;set;}=null;
    /// <summary>
    /// If relevant what is the string, integer or decimal value of this thing
    /// </summary>
    public double? decimalValue {get;set;}=null;
    /// <summary>
    /// If relevant what is the string, integer or decimal value of this thing
    /// </summary>
    public string? stringValue {get;set;}=null;
}