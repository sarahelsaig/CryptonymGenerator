namespace CryptonymGenerator.Services
{
    /// <summary>
    /// A factory class to create new <see cref="CryptonymGenerator"/> instance from dependency injection.
    /// </summary>
    public interface ICryptonymGeneratorFactory
    {
        /// <summary>
        /// Returns a new instance of <see cref="CryptonymGenerator"/> with the given settings.
        /// </summary>
        CryptonymGenerator Create(string? language = "en", string? partOfSpeech = "noun");
    }
}