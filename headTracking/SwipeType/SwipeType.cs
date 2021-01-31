using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace headTracking.SwipeType
{
    /// <summary>
    /// Abstract SwipeType.
    /// </summary>
    public abstract class SwipeType
    {
        /// <summary>
        /// </summary>
        /// <param name="wordList">The dictionary of words.</param>
        protected SwipeType(string[] wordList) { Words = wordList; }

        /// <summary>
        /// Dictionary of words.
        /// </summary>
        public string[] Words { get; }

        /// <summary>
        /// Returns suggestions for an input string.
        /// </summary>
        /// <param name="input">Input string</param>
        public IEnumerable<string> GetSuggestion(string input)
        {
            return GetSuggestion(input, -1);
        }

        /// <summary>
        /// Returns suggestions for an input string.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="count">The number of elements to return.</param>
        public IEnumerable<string> GetSuggestion(string input, int count)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Enumerable.Empty<string>();
            }
            var suggestion = GetSuggestionImpl(input.ToLower(CultureInfo.InvariantCulture));
            if (count > 0)
            {
                return suggestion.Take(count);
            }
            return suggestion;
        }

        /// <summary>
        /// Returns suggestions for an input string.
        /// </summary>
        /// <param name="input">Input string</param>
        protected abstract IEnumerable<string> GetSuggestionImpl(string input);
    }
}
