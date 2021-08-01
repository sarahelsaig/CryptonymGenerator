using System.IO;

namespace CryptonymGenerator.Services
{
    public class CryptonymGeneratorFactory : ICryptonymGeneratorFactory
    {
        private readonly IDictionaryManager _dictionaryManager;

        public CryptonymGeneratorFactory(IDictionaryManager dictionaryManager) => 
            _dictionaryManager = dictionaryManager;

        public CryptonymGenerator Create(string? language = "en", string? partOfSpeech = "noun")
        {
            var prefixDictionaryPath = DictionaryManager.GetSavePath(language, partOfSpeech, "prefixes");
            var codeWordDictionaryPath = DictionaryManager.GetSavePath(language, partOfSpeech, "code", "words");
            
            var dictionaryPath = Path.GetDirectoryName(prefixDictionaryPath) ?? string.Empty;
            if (!Directory.Exists(dictionaryPath)) Directory.CreateDirectory(dictionaryPath);
                
            return new CryptonymGenerator(
                _dictionaryManager,
                language,
                partOfSpeech,
                prefixDictionaryPath,
                codeWordDictionaryPath);
        }
    }
}