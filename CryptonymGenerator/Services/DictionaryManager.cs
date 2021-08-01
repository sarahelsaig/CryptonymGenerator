using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CryptonymGenerator.Services
{
    public class DictionaryManager : IDictionaryManager
    {
        private readonly IWordDownloader _downloader;

        private readonly Dictionary<(string?, string?), IList<string>> _words = new();

        public DictionaryManager(IWordDownloader downloader) => _downloader = downloader;

        public Task<IList<string>> GetAsync(string? language, string? partOfSpeech)
        {
            if (language?.ToUpperInvariant() == "ALL") language = null;
            if (partOfSpeech?.ToUpperInvariant() == "ALL") partOfSpeech = null;
            
            var key = (language?.ToUpperInvariant(), partOfSpeech?.ToUpperInvariant());
            return _words.TryGetValue(key, out var storedWords)
                ? Task.FromResult(storedWords) 
                : GetInnerAsync(language, partOfSpeech, key);
        }

        private async Task<IList<string>> GetInnerAsync(string? language, string? partOfSpeech, (string?, string?) key)
        {
            var savePath = GetSavePath(language, partOfSpeech, "words");
            if (!File.Exists(savePath))
            {
                await _downloader.DownloadAllWords(savePath, language, partOfSpeech);
            }

            var words = await File.ReadAllLinesAsync(savePath, Encoding.UTF8);
            _words[key] = words;
            return words;
        }

        public static string GetSavePath(string? language, string? partOfSpeech, params string[] fileName) =>
            Path.Join(
                    "data", 
                    language ?? "all",
                    partOfSpeech ?? "all",
                    string.Join(".", fileName) + ".txt")
                .ToLowerInvariant();
    }
}