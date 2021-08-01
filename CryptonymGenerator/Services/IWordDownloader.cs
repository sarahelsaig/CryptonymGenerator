using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptonymGenerator.Services
{
    /// <summary>
    /// A service for downloading and storing a collection of words from a given language and of a given part of speech.
    /// </summary>
    public interface IWordDownloader
    {
        /// <summary>
        /// Downloads the available words into a string collection.
        /// </summary>
        /// <param name="language">The two letter language code for the word or <see langword="null"/> for all.</param>
        /// <param name="partOfSpeech">A constraint to limit results to a specific part of speech or <see langword="null"/> for all.</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllWords(
            string? language,
            string? partOfSpeech);

        /// <summary>
        /// Saves the results from <see cref="GetAllWords"/> into a text file.
        /// </summary>
        /// <param name="filePath">The location and file name where to save the text file.</param>
        /// <param name="language">The two letter language code for the word or <see langword="null"/> for all.</param>
        /// <param name="partOfSpeech">A constraint to limit results to a specific part of speech or <see langword="null"/> for all.</param>
        /// <returns></returns>
        Task DownloadAllWords(
            string filePath,
            string? language,
            string? partOfSpeech);
    }
}