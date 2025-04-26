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
                    if (!synonyms.ContainsKey(key))
                        synonyms.Add(key, new HashSet<string>(new string[]{synonym}));
                    else
                        synonyms[key].Add(synonym);
                    //If A is B then B is A
                    if (!synonyms.ContainsKey(synonym))
                        synonyms.Add(synonym, new HashSet<string>(new string[]{key}));
                    else
                        synonyms[synonym].Add(key);
                }
            }

    }

        /// <summary>
        /// Get all potential synonyms of this string, by looking for any matching synonyms anywhere
        /// ISSUE: this only matches the FIRST instance of a synonym, this is GOOD ENOUGH in this case
        /// Synonyms will NOT be checked in the middle of words, for instance the word "som" (which in Danish) will not be substituted with "grism" even though "so" (female pig) is a possible synonym with "gris" (pig) (According to Den Danske Ordbog)
        /// </summary>
        /// <param name="input">A string which can be a sentence, or a single word</param>
        /// <returns>List of strings which mean the same</returns>
        public List<string> getSimilar(string input)
        {
            List<string> similar= new List<string>();


            //Since synonyms should not be checked inside words, we will look at 
            //Create all possible substrings which does not split words, stored as start id and length
            List<(int,int)> substrings=new();
            //First split into words, this makes us ignore non-word caracters like punctuation
            string[] words = Regex.Split(input, @"\W");
            Console.WriteLine(""+words.Length);
            int id0=0;
            for (int i = 0; i < words.Length; ++i)
            {
                int id1=id0;
                for (int j = i; j < words.Length; ++j)
                {
                    substrings.Add((id0,id1+words[j].Length-id0));
                    id1+=words[j].Length;
                    Console.WriteLine(input.Substring(id0,id1-id0));
                    id1+=1;
                } 
                id0+=1+words[i].Length;
            }
            return similar;
            
            //We have to loop through all synonyms, and check if they are contained within the input
            foreach (string Key in synonyms.Keys)
            {
                //Id and length of the best match, if any
                int id;
                int length;
                //No need to do a fuzzy search, if there is an exact match
                if (input.Contains(Key))
                {
                    id = input.IndexOf(Key);
                    length = input.Length - id;
                }
                else
                {
                    
                    id = 0;
                    length = input.Length;
                }

                //Synonyms can not be found inside words, so if this is not at the start, check that there isn't another letter in front
                if (id>0)
                {
                    //If this is part of a word, skip it
                    char C =input[id-1];
                    if (Char.IsLetter(C))
                        continue;
                }
                if (id+length<input.Length)
                {
                    //If this is part of a word in the other direction, skip it
                    char C =input[id+length];
                    if (Char.IsLetter(C))
                        continue;
                }

                string sub = input.Substring(id,length);

                foreach (string Replacement in synonyms[Key])
                {
                    similar.Add(input.Substring(0,id)+Replacement+input.Substring(id+length));
                }
                
            }


            return similar;
        }
        /// <summary>
        /// Return true if <paramref name="That"/> is synonymous with <paramref name="That"/>?
        /// </summary>
        /// <param name="This"></param>
        /// One word
        /// <param name="That"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// Other word
        /// <param name="That"></param>
        /// <returns></returns>
        public bool IsSame(string This, string That)
        {
            //Finvalsede havregryn = 
            //Find the best match in the keys

            var Out = Process.ExtractTop(This, synonyms.Keys);
            foreach (var w in Out)
                Console.WriteLine($"{w.Index} {w.Score} {w.Value}");
            return false;
        }

}