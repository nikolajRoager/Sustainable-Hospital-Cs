using System.IO;
using System.Text.RegularExpressions;
using OfficeOpenXml.Packaging.Ionic.Zip;
using FuzzySharp;
using System.Text;

///<summary>
/// A synonym dictionary, is a dictionary which contains pairs of words or phrases and their synonyms for known words
///</summary>
public class SynonymDictionary
{
    ///<summary>
    /// The actual data, phrase and set of synonyms, all saved as strings (regices can't be used in dictionaries)
    ///</summary>
    private Dictionary<string, HashSet<string> > synonyms;

    ///<summary>
    /// Load from a file such as ddo-synonyms.csv (descriped in readme.md)
    /// 
    ///</summary>
    ///<param name="Ignore">
    /// Ignore all entries in the dictionary with these strings (the dictionary may regex, or other generalized structures, which are not relevant in our case)
    ///</param>
    /// <param name="dictionaryPath">
    /// File to load the dictionary from, it is assumed it is a text file in the format AkBsCsD
    /// where A, B, C ... is a string k is <paramref name="keySep"/> and s is <paramref name="synSep"/>
    /// It is understand as A being synonymous with B, A being synonymous with C, etc. but not B with C (unless explicitly specified)
    /// </param>
    /// <param name="keySep">
    /// Char separating the keyword from the synonym
    /// </param>
    /// <param name="synSep">
    /// Char separating the synonyms from each other
    /// </param>
    public SynonymDictionary(string dictionaryPath,char keySep, char synSep, String[] Ignore)
    {
        synonyms = new Dictionary<string, HashSet<string>>();

        //This function uses buffering to load the file line by line efficiently
        foreach (string line in File.ReadLines(dictionaryPath))
            if (line!=null)          
            {
                string key;
                string[] synonym_list;

                //This works regardless if keySep = synSep or not, as long as the key is the first word
                {
                    string[] splitted=line.Split(keySep,2);
                    if (splitted.Length<=1 || splitted[0].Length==0)//only key, or nothing ... just skip, no synonyms
                        continue;
                    else
                    {
                        key = splitted[0];
                        synonym_list = splitted[1].Split(synSep);
                    }
                }

                bool ignoreThis=false;
                //Check if the key contains some kind of special symbol (i.e. it is some kind of Regex), if so ignore it
                foreach (string ignore in Ignore)
                {
                    if (key.Contains(ignore))
                    {
                        ignoreThis = true;
                        continue;
                    }
                }
                if (ignoreThis)
                    continue;

                //Now, add each non-ignored synonym to the list of synonyms for this keyword
                foreach (string synonym in synonym_list)
                {
                    bool ignoreSyn=false;
                    foreach (string ignore in Ignore)
                    {
                        if (synonym.Contains(ignore))
                        {
                            ignoreSyn = true;
                            continue;
                        }
                    }
                    if (ignoreSyn)
                        continue;
                    
                    //If the entry for these words don't already exist, make it exist
                    if (!synonyms.ContainsKey(key.ToLower()))
                        synonyms.Add(key.ToLower(), new HashSet<string>(new string[]{synonym}));
                    else
                        synonyms[key.ToLower()].Add(synonym);
                    //If A is B then B is A
                    if (!synonyms.ContainsKey(synonym.ToLower()))
                        synonyms.Add(synonym.ToLower(), new HashSet<string>(new string[]{key}));
                    else
                        synonyms[synonym.ToLower()].Add(key.ToLower());
                }
            }

    }

        /// <summary>
        /// Get all potential synonyms of this string, by looking for any matching synonyms anywhere
        /// ISSUE: this only matches the FIRST instance of a synonym, this is GOOD ENOUGH in this case
        /// ISSUE fuzzy text search is highly unreliable, finding ridiculously broad matches
        /// Synonyms will NOT be checked in the middle of words, for instance the word "som" (which in Danish) will not be substituted with "grism" even though "so" (female pig) is a possible synonym with "gris" (pig) (According to Den Danske Ordbog)
        /// </summary>
        /// <param name="input">A string which can be a sentence, or a single word</param>
        /// <returns>List of strings which mean the same</returns>
        public List<string> getSimilar(string input, bool fuzzy=false)
        {
            List<string> similar= new List<string>();


            //Since synonyms should not be checked inside words, we will look at 
            //Create all possible substrings which does not split words, stored as start id and length
            List<(int,int)> substrings=new();
            //First split into words, this makes us ignore non-word caracters like punctuation
            string[] words = Regex.Split(input, @"\W");
            int id0=0;
            for (int i = 0; i < words.Length; ++i)
            {
                int id1=id0;
                for (int j = i; j < words.Length; ++j)
                {
                    substrings.Add((id0,id1+words[j].Length-id0));
                    id1+=words[j].Length;
                    id1+=1;
                } 
                id0+=1+words[i].Length;
            }

            //Now loop through
            foreach ( (int start, int length) in substrings)
            {
                //Use fuzzy text comparison when looking for synonyms (SLOOOW)
                if (fuzzy)
                {
                    var bestMatch = Process.ExtractOne(input.Substring(start,length).ToLower(),synonyms.Keys,null,null,1);
                    if (bestMatch!=null && bestMatch.Score>=90)
                    {
                        //Close enough! add all synonyms
                        foreach (string Replacement in synonyms[bestMatch.Value])
                        {
                            similar.Add(input.Substring(0,start)+Replacement+input.Substring(start+length));
                        }
                    }
                }
                //Only look for synonyms with literal match
                else
                {
                    string toReplace =input.Substring(start,length).ToLower();
                    if (synonyms.Keys.Contains(toReplace))
                        foreach (string Replacement in synonyms[toReplace])
                        {
                            similar.Add(input.Substring(0,start)+Replacement+input.Substring(start+length));
                        }

                }
            }
            return similar;
        }
        /// <summary>
        /// Return true if <paramref name="That"/> is the same as <paramref name="That"/>?
        /// </summary>
        /// <param name="This"></param>
        /// One string to compare
        /// <param name="That">
        /// Other string to compare
        /// </param>
        /// <param name="synonym">
        /// Look for synonyms (signficantly slower), default true
        /// </param>
        /// <param name="fuzzy">
        /// Use fuzzy text comparison (slower, and at the moment so broad as to be borderline useless), default false
        /// </param>
        /// <returns></returns> 
        public bool IsSame(string This, string That,bool synonym=true,bool fuzzy=false)
        {
            if (!synonym && !fuzzy)
                return This.ToLower().Equals(That.ToLower());
            if (!synonym && fuzzy)
            {
                return Fuzz.Ratio(This, That)>=90;
            }
            else
            {
                //Get all different ways of writing this or that, and compare them to each other
                var SimilarToThis = getSimilar(This,fuzzy);
                SimilarToThis.Add(This);
                var SimilarToThat = getSimilar(That,fuzzy);
                SimilarToThat.Add(That);

                foreach (var A in SimilarToThis)
                    foreach (var B in SimilarToThat)
                        if (A.ToLower().Equals(B.ToLower()))
                        {
                            return true;
                        }
                return false;
            }
        }
}