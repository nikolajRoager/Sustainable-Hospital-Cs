public class OutputTable
{
    public List<OutputEntry> lines { get; set; }

    public int numberCol { get; set; } = -1;//-1 = not found
    public int amountCol{ get; set; } = -1;
    public int productCol{ get; set; } = -1;
    public int singleMassCol{ get; set; } = -1;
    public int totalMassCol { get; set; } = -1;
    //Value of the output table, set from analyzer
    public int value { get; set; }
    public OutputTable()
    {
        lines = new List<OutputEntry>();
    }
}