using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptonymGenerator.Services
{
    /// <summary>
    /// A service for retrieving various word listings by language and part of speech.
    /// </summary>
    public interface IDictionaryManager
    {
        /// <summary>
        /// Returns the selected word listing from memory, a stored file, or a web API in that order or precedence. 
        /// </summary>
        /// <param name="language">
        /// The two letter language code for the word. It's case-insensitive. Use the text <c>all</c> or
        /// <see langword="null"/> to not filter by this criteria.</param>
        /// <param name="partOfSpeech">
        /// A constraint to limit results to a specific part of speech. It's case-insensitive. Use the text <c>all</c>
        /// or <see langword="null"/> to not filter by this criteria.</param>
        /// <returns>The list of words.</returns>
        Task<IList<string>> GetAsync(string? language, string? partOfSpeech);
    }
}