namespace StringAnalyzer
{
    /// <summary>
    /// A product we can work with, has a keyword (its name), material (what it is made of: råvare), and category
    /// </summary>
    public interface IProduct  : IComparable<IProduct>
    {
        /// <summary>
        /// Who am I (the keyword or name of the specific product)
        /// </summary>
        public string Keyword { get; set; }
        /// <summary>
        /// What am I (what this thing is made of, also known as ingredient)
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// Where do I belong (What category of products this belongs in)
        /// </summary>
        public string Category { get; set; }
    }
}