namespace RegexProductFinder
{
    /// <summary>
    /// A keyword paired with an ingredient and a category for the ingredient
    /// Sorted by length of word, although exact matches are weighted higher than those containing regex wildcards
    /// </summary>
    public class RegexProductPair : IComparable<RegexProductPair>
    {
        public string keyword {get;set;}
        public string ingredient {get;set;}
        public string category {get;set;}

        public RegexProductPair(string category, string ingredient, string keyword)
        {
            this.keyword=keyword.ToLower();
            this.ingredient=ingredient.ToLower();
            this.category=category.ToLower();
        }

        public int CompareTo(RegexProductPair other)
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