/// <summary>
/// A keyword paired with an ingredient and a category for the ingredient
/// Sorted by length of word, although exact matches are weighted higher than those containing regex wildcards
/// </summary>
class RegexProductPair
{
    public string keyword {get;set;}
    public string ingredient {get;set;}
    public string category {get;set;}

    RegexProductPair(string keyword, string ingredient, string category)
    {
        this.keyword=keyword;
        this.ingredient=ingredient;
        this.category=category;
    }
}