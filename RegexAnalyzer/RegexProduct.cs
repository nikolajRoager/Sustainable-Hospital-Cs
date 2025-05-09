using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using StringAnalyzer;
namespace RegexAnalyzer
{
    /// <summary>
    /// A product we can work with, has a keyword (its name), material (what it is made of: råvare), and category
    /// </summary>
    public class RegexProduct : IProduct
    {
        /// <summary>
        /// Who am I (the keyword or name of the specific product)
        /// </summary>
        [JsonPropertyName("Keyword")]
        public string Keyword { get; set; }
        /// <summary>
        /// What am I (what this thing is made of, also known as ingredient)
        /// </summary>
        [JsonPropertyName("Material")]
        public string Material { get; set; }
        /// <summary>
        /// Where do I belong (What category of products this belongs in)
        /// </summary>
        [JsonPropertyName("Category")]
        public string Category { get; set; }

        /// <summary>
        /// The regex created from the keyword on creation or deserialization
        /// </summary>
        //This is the keyword converted to Json anyway, just ignore it
        [JsonIgnore]
        public Regex KeyRegex{ get; set; }
        
        /// <summary>
        /// Generate the keyword regex,
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Console.WriteLine("DESERIALIZE "+Keyword.ToLower());
            KeyRegex = new Regex(Keyword.ToLower(),RegexOptions.IgnoreCase);
        }
        

        /// <summary>
        /// Create a pair based on this category, ingredient and keyword
        /// Throws ArgumentException or ArgumentNullException if keyword is not a regex
        /// </summary>
        /// <param name="category"></param>
        /// <param name="Material"></param>
        /// <param name="keyword"></param>
        public RegexProduct(string category, string Material, string keyword)
        {
            this.Keyword=keyword;
            Console.WriteLine("NODESERIALIZE "+Keyword.ToLower());
            this.KeyRegex = new Regex(keyword.ToLower(),RegexOptions.IgnoreCase);
            this.Material=Material.ToLower();
            this.Category=category.ToLower();
        }

        public string[] keyWordList ()
        {
            return Keyword.Split(" ,.*?",StringSplitOptions.RemoveEmptyEntries);
        }

        public int CompareTo(IProduct? other)
        {
            if (other == null) return 1;

            //Longer words have higher priority, as otherwise shorter words would catch everythin
            int result = other.Keyword.Length.CompareTo(Keyword.Length);
            if (result == 0)
            {
                result = string.Compare(Keyword,other.Keyword,StringComparison.CurrentCulture);
            }
            return result;
        }
    }
}