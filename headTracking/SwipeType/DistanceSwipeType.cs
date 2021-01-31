using System.Collections.Generic;
using System.Linq;

namespace headTracking.SwipeType
{
    /// <summary>
    /// SwipeType using Damerau–Levenshtein distance.
    /// </summary>
    public class DistanceSwipeType : SwipeType
    {
        /// <summary>
        /// </summary>
        /// <param name="wordList">The dictionary of words.</param>
        public DistanceSwipeType(string[] wordList) : base(wordList) { }

        /// <summary>
        /// Returns suggestions for an input string.
        /// </summary>
        /// <param name="input">Input string</param>
        protected override IEnumerable<string> GetSuggestionImpl(string input)
        {
            return Words.OrderBy(x => TextDistance.GetDamerauLevenshteinDistance(input, x));
        }
    }
}
