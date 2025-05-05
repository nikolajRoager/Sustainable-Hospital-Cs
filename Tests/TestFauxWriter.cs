namespace Tests;
using Services;

public class TestFauxWriter
{
    //Test that the faux writer works as expected, otherwise we can't trust any other tests
    [Fact]
    public void TestWriter()
    {
        var writer = new FauxWriter();
        writer.writeLine("Lorem Ipsum");
        writer.writeLine("Dolor Sit Amat");
        int len = writer.lines.Count;
        Assert.Equal(2,len);
        Assert.Equal("Lorem Ipsum",writer.lines[0]);
        Assert.Equal("Dolor Sit Amat",writer.lines[1]);
    }
}
