using System.Text.RegularExpressions;

namespace RegexProductFinder
{
    /// <summary>
    /// A keyword paired with an ingredient and a category for the ingredient
    /// Sorted by length of word, although exact matches are weighted higher than those containing regex wildcards
    /// </summary>
    public class RegexProductPair : IComparable<RegexProductPair>
    {
        public string keyword {get;set;}
        public Regex keyRegex {get;set;}
        public string ingredient {get;set;}
        public string category {get;set;}

        /// <summary>
        /// Create a pair based on this category, ingredient and keyword
        /// Throws ArgumentException or ArgumentNullException if keyword is not a regex
        /// </summary>
        /// <param name="category"></param>
        /// <param name="ingredient"></param>
        /// <param name="keyword"></param>
        public RegexProductPair(string category, string ingredient, string keyword)
        {
            this.keyRegex = new Regex(keyword,RegexOptions.IgnoreCase);
            this.keyword=keyword;
            this.ingredient=ingredient.ToLower();
            this.category=category.ToLower();
        }

        public int CompareTo(RegexProductPair? other)
        {
            if (other == null) return 1;

            //Longer words have higher priority, as otherwise shorter words would catch everythin
            int result = other.keyword.Length.CompareTo(keyword.Length);
            if (result == 0)
            {
                result = string.Compare(keyword,other.keyword,StringComparison.CurrentCulture);
            }
            return result;
        }
    }
}