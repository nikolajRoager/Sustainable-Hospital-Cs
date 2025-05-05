namespace Tests;
using Services;

public class TestFauxReader
{
    //Test that the faux reader works as expected, otherwise we can't trust any other tests
    [Fact]
    public void TestReader()
    {
        var reader = new FauxReader(["Lorem Ipsum","Dolor Sit Amat"]);
        
        var lines = reader.ReadLines().ToList();
        Assert.Equal(2,lines.Count);
        Assert.Equal("Lorem Ipsum",lines[0]);
        Assert.Equal("Dolor Sit Amat",lines[1]);
    }
}
