/// <summary>
/// An entry in the final output
/// </summary>
public class OutputEntry
{
    public string productNameFull { get; set; } = "null";
    public string productName { get; set; } = "null";
    public string category    { get; set; } = "null";
    public string material { get; set; } = "null";
    public string number { get; set; } = "null";
    public double amount { get; set; } = 0;
    public double totalMass { get; set; } = 0;
    public double singleMass { get; set; } = 0;
}