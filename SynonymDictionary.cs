using System.IO;
using System.Text.RegularExpressions;

///<summary>
/// A synonym dictionary, is a dictionary which contains pairs of words or phrases and their synonyms for known words
///</summary>
public class SynonymDictionary
{
    ///<summary>
    /// The actual data, phrase and set of synonyms, all saved as a 
    ///</summary>
    private Dictionary<string, Set<string> > synonyms;

    ///<summary>
    /// Load from a file with the same format as ddo-synonyms.csv (descriped in readme.md)... or in the comments here
    ///</summary>
    public SynonymDictionary(string dictionaryPath)
    {
        string extractKeyRegexString = @"^(?<key>[^\t]+)\t(?<synonyms>.+)";
        Regex extractKeyRegex;

        //We need to detect the strings in the patterns which can be replaced with anything
        // nogen/noget -> * + entry
        // .. -> * + entry
        // A(B)C -> AC or ABC
        // AB/CD -> ABD or ACD

        //Since the dictionary file is likely compiled from a dictionary written by humans, formatting errors are expected, and should be skipped


        //The Regex which extracts optional elements
        //For example the default turns "(alt) i alt" into "?(?:alt).?i alt", the latter of which is a valid rege
        string MakeRegexRegexString ="";

        try
        {
            extractKeyRegex = new(extractKeyRegexString);
        }
        catch (ArgumentException E)
        {
            throw new ArgumentException($"Den brugerdefinerede REGEX:\"{extractKeyRegexString}\" er ikke gyldig, REGEX fejlmeddelelse: {E.Message}");
        }
        char synonymSeparator = ';';

        int max = 10;
        //This function uses buffering to load the file line by line efficiently
        foreach (string line in File.ReadLines(dictionaryPath))
            if (line!=null)          
            {
                //Extract key group and list of all synonyms
                Console.WriteLine($"\"{line}\"");
                Match KeySynonymsPair;
                try
                {
                    KeySynonymsPair = extractKeyRegex.Match(line);
                    extractKeyRegex = new(extractKeyRegexString);
                }
                //This can not happen, we know line is not null, but this removes compiler warnings
                catch (ArgumentNullException E)
                {
                    throw new ArgumentException($"Den brugerdefinerede REGEX:\"{extractKeyRegexString}\" returnerede fejl på linjen \"{line}\"!, ArgumentNullException");
                }
                catch (RegexMatchTimeoutException E)
                {
                    throw new ArgumentException($"Den brugerdefinerede REGEX:\"{extractKeyRegexString}\" returnerede fejl på linjen \"{line}\"!, REGEX tog for lang tid");
                }
                {

                    var keyCapture = KeySynonymsPair.Groups[0].Captures;
                    if (keyCapture.Count==0)
                        throw new ArgumentException($"Ingen match til første gruppe i REGEX:\"{extractKeyRegexString}\" på linjen \"{line}\"!, REGEX tog for lang tid");
                    string thisKey = keyCapture[0];

                }
                --max;
                if (max<=0)
                    break;
                

            }
        
        //The dictionary key and synonyms are supposed to be a REGEX, 

        //The user can supply any regex search and replace pattern, which will be run on all dictionary entries
        //this way, the user can force a dictionary which doesn't comply
        //string regex_find_key =
    }

}