using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexicala.NET;
using Lexicala.NET.Request;

namespace CryptonymGenerator.Services
{
    public class WordDownloader : IWordDownloader
    {
        private readonly ILexicalaClient _client;

        public WordDownloader(ILexicalaClient client) => _client = client;

        public async Task<IEnumerable<string>> GetAllWords(
            string? language,
            string? partOfSpeech)
        {
            var request = new AdvancedSearchRequest
            {
                Language = language,
                Pos = partOfSpeech,
                Number = "singular",
                PageLength = 30,
            };

            var results = new List<string>();

            bool isMore = true;
            while (isMore)
            {
                var response = await _client.AdvancedSearchAsync(request);
                results.AddRange(
                    response
                        .Results
                        .Select(result =>
                            result?.Headword.Headword?.Text ??
                            result?.Headword.HeadwordElementArray?.First()?.Text ??
                            string.Empty)
                        .Where(result => !string.IsNullOrWhiteSpace(result)));
                
                isMore = request.Page < response.NPages;
                request.Page++;
            }

            return results;
        }

        public async Task DownloadAllWords(
            string filePath,
            string? language,
            string? partOfSpeech)
        {
            var words = (await GetAllWords(language, partOfSpeech)).Select(word => word.LettersOnlyAsUpperCase());
            
            await using var writer = new StreamWriter(filePath, append: false, Encoding.UTF8);
            foreach (var word in words) await writer.WriteLineAsync(word);
        }
    }
}