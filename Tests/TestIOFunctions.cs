using Services;

public class TestIOFunctiosn
{
    //Test that the genuine file reader and writer works
    [Fact]
    public void TestIO()
    {
        //This should be OS independent, so it should work on automated frameworks (Assuming we have read-write permission)
        string tempfile = Path.GetTempFileName();
        //Simply write some filler to a temporary file
        var writer = new FileWriter(tempfile);
        writer.writeLine("Lorem Ipsum");
        writer.writeLine("Dolor Sit Amat");
        writer.save();

        //and read it again
        var reader = new FileReader(tempfile);
        var lines = reader.ReadLines().ToList();
        int len = lines.Count;
        Assert.Equal(2,len);
        Assert.Equal("Lorem Ipsum",lines[0]);
        Assert.Equal("Dolor Sit Amat",lines[1]);
    }
}