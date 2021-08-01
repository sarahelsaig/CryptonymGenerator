using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptonymGenerator.Services;

namespace CryptonymGenerator
{
    public class CryptonymGenerator
    {
        private readonly IDictionaryManager _dictionaryManager;
        private readonly string? _language;
        private readonly string? _partOfSpeech;
        private readonly string _prefixDictionaryPath;
        private readonly string _codeWordDictionaryPath;

        private bool _isPrepared;

        public List<string> Words { get; } = new();
        public List<(string Code, string Name)> Prefixes { get; } = new();
        public Dictionary<string, string> CodeWords { get; } = new();

        public CryptonymGenerator(
            IDictionaryManager dictionaryManager,
            string? language,
            string? partOfSpeech,
            string prefixDictionaryPath,
            string codeWordDictionaryPath)
        {
            _dictionaryManager = dictionaryManager;
            _language = language;
            _partOfSpeech = partOfSpeech;
            _prefixDictionaryPath = prefixDictionaryPath;
            _codeWordDictionaryPath = codeWordDictionaryPath;
        }

        public async Task PrepareAsync()
        {
            Words.AddRange(await _dictionaryManager.GetAsync(_language, _partOfSpeech));

            if (string.IsNullOrWhiteSpace(_prefixDictionaryPath)) throw new InvalidOperationException("The file path is missing!");
            
            Prefixes.AddRange(
                File.Exists(_prefixDictionaryPath)
                    ? (await File.ReadAllLinesAsync(_prefixDictionaryPath, Encoding.UTF8))
                        .Where(line => line.Length > 2 && line[2] == ':')
                        .Select(line => (line[..2], line[3..]))
                        .ToList()
                    : Words
                        .GroupBy(word => word[..2].ToUpperInvariant())
                        .Select(group => new { group.Key, Count = group.Count() })
                        .OrderByDescending(group => group.Count)
                        .Select(group => (group.Key, string.Empty))
                        .ToList());

            if (File.Exists(_codeWordDictionaryPath))
            {
                CodeWords.AddRange(
                    (await File.ReadAllLinesAsync(_codeWordDictionaryPath, Encoding.UTF8))
                    .Where(line => line.Contains(':'))
                    .ToDictionary(
                        line => line[..line.IndexOf(':')],
                        line => line[(line.IndexOf(':') + 1)..]));
            }

            await SaveAsync();
            _isPrepared = true;
        }

        public async Task<string> ClaimPrefixAsync(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new InvalidOperationException($"The {nameof(displayName)} can not be empty.");
            }

            displayName = displayName.ToUpperInvariant();
            
            if (!_isPrepared) await PrepareAsync();

            for (var i = 0; i < Prefixes.Count; i++)
            {
                var (code, name) = Prefixes[i];
                if (!string.IsNullOrWhiteSpace(name)) continue;
                
                Prefixes[i] = (code, displayName);
                await SaveAsync();
                return code;
            }

            throw new IndexOutOfRangeException("We've ran out of available prefixes!");
        }

        public async Task<string> ClaimCodeWordAsync(string prefix, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new InvalidOperationException($"The {nameof(displayName)} can not be empty.");
            }
            
            displayName = displayName.ToUpperInvariant();
            
            if (!_isPrepared) await PrepareAsync();
            
            // Find and purify word.
            var word = Words[new Random().Next(Words.Count)].LettersOnlyAsUpperCase();
            
            // Skip if it's already in use.
            if (CodeWords.ContainsKey(prefix + word) ||
                CodeWords.ContainsKey(prefix + word[1..]) ||
                CodeWords.ContainsKey(prefix + word[2..]))
            {
                return await ClaimCodeWordAsync(prefix, displayName);
            }

            // Merge words on (somewhat) matching characters.
            if (word[0] == prefix[0] && prefix[0] != prefix[1])
            {
                word = prefix + word[2..];
            } 
            else if (word[1] == prefix[0])
            {
                word = prefix + word[1..];
            }
            else
            {
                word = prefix + word;
            }

            // Save.
            CodeWords[word] = displayName;
            await SaveAsync();
            return word;
        }

        private async Task SaveAsync()
        {
            await File.WriteAllLinesAsync(
                _prefixDictionaryPath,
                Prefixes.Select(pair => $"{pair.Code}:{pair.Name}"),
                Encoding.UTF8);
            
            await File.WriteAllLinesAsync(
                _codeWordDictionaryPath!,
                CodeWords.Select(pair => $"{pair.Key}:{pair.Value}"),
                Encoding.UTF8);
        }
    }
}