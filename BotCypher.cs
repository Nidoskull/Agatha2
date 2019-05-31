using System.Collections.Generic;
using System.Globalization;

namespace Agatha2
{	
	internal abstract class BotCypher
	{
		internal Dictionary<string, List<string>> substitution = new Dictionary<string, List<string>>();
		internal string ApplySubstition(string incoming)
		{
			foreach(KeyValuePair<string, List<string>> cypherList in substitution)
			{
				foreach(string cypherChar in cypherList.Value)
				{
					incoming = incoming.Replace(cypherChar, cypherList.Key, false, CultureInfo.CurrentCulture);
					incoming = incoming.Replace(cypherChar.ToUpper(), cypherList.Key.ToUpper(), false, CultureInfo.CurrentCulture);
				}
			}
			return incoming;
		}
		internal virtual string ApplyPreSubstitution(string incoming)
		{
			return incoming;
		}
		internal virtual string ApplyPostSubstitution(string incoming)
		{
			return incoming;
		}
	}

	internal class CypherProfanityFilter : BotCypher
	{
		public CypherProfanityFilter()
		{
			substitution.Add(":bee:", new List<string> {"fuck", "shit"});
		}
	}
}