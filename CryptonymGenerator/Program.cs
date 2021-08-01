using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptonymGenerator.Services;
using Lexicala.NET;
using Lexicala.NET.Configuration;
using Lexicala.NET.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace CryptonymGenerator
{
    class Program
    {
        private static string Prompt(string text)
        {
            Console.Write("{0}", text);
            return Console.ReadLine() ?? string.Empty;
        }
        
        private static string PromptUntil(string text, Func<string, bool>? predicate = null)
        {
            string? result = null;
            while (string.IsNullOrWhiteSpace(result) || (predicate != null && !predicate(result!)))
            {
                result = Prompt(text);
            }

            return result!;
        }
        
        private static void PromptEnter() => Prompt("Press Enter to continue...");

        private static void Title(string text)
        {
            Console.WriteLine(text.ToUpperInvariant());
            foreach (var _ in text) Console.Write("*");
            Console.WriteLine("\n");
        }

        private static IEnumerable<(string Code, string Name)> GetOccupiedPrefixes(CryptonymGenerator generator) =>
            generator.Prefixes.Where(item => !string.IsNullOrWhiteSpace(item.Name));

        private static void ListCryptonymPrefixes(CryptonymGenerator generator)
        {
            Title("CRYPTONYM PREFIXES");
            foreach (var (code, name) in GetOccupiedPrefixes(generator)) Console.WriteLine("{0}: {1}", code, name);
            PromptEnter();
        }

        private static void ListCryptonyms(CryptonymGenerator generator)
        {
            Title("CRYPTONYMS");

            var prefixDictionary = generator.Prefixes.ToDictionary(pair => pair.Code, pair => pair.Name);
                        
            foreach (var (code, name) in generator.CodeWords)
            {
                Console.WriteLine("{0}: {1} ({2})", code, name, prefixDictionary[code[0..2]]);
            }

            PromptEnter();
        }

        private static async Task<string> ClaimCryptonym(CryptonymGenerator generator)
        {
            var occupiedPrefixes = GetOccupiedPrefixes(generator).ToList();
            for (var i = 0; i < occupiedPrefixes.Count; i++)
            {
                var (code, name) = occupiedPrefixes[i];
                Console.WriteLine("{0}. {1}: {2}", i + 1, code, name);
            }
                        
            var prefixIndex = PromptUntil(
                    "Select prefix: ",
                    word => int.TryParse(word, out var num) && num > 0 && num <= occupiedPrefixes.Count)
                .ParseInt();
            var prefixName = occupiedPrefixes[prefixIndex - 1].Code; 
            var codeWordName = PromptUntil("Enter a description: "); 
            return await generator.ClaimCodeWordAsync(prefixName, codeWordName);
        }
        
        public static async Task Main(string[] args)
        {
            var config = args.Length > 1
                ? new LexicalaConfig(args[0], args[1])
                : new LexicalaConfig(Prompt("User Name: "), Prompt("Password: "));
            var language = args.Length > 2 ? args[2] : "en";
            var partOfSpeech = args.Length > 3 ? args[3] : "noun";

            var services = new ServiceCollection();
            services.AddScoped<IWordDownloader, WordDownloader>();
            services.AddScoped<IDictionaryManager, DictionaryManager>();
            services.AddScoped<ICryptonymGeneratorFactory, CryptonymGeneratorFactory>();
            
            
            services.AddHttpClient<ILexicalaClient, LexicalaClientAlt>((Action<HttpClient>) (client =>
            {
                client.BaseAddress = LexicalaConfig.BaseAddress;
                client.DefaultRequestHeaders.Authorization = config.CreateAuthenticationHeader();
            }));
            services.AddMemoryCache();
            services.AddSingleton<ILexicalaSearchParser, LexicalaSearchParser>();
            
            await using var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<ICryptonymGeneratorFactory>();

            var generator = factory.Create(language, partOfSpeech);
            await generator.PrepareAsync();
            
            var message = string.Empty;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. LIST CRYPTONYM PREFIXES");
                Console.WriteLine("2. LIST CRYPTONYMS");
                Console.WriteLine("3. CLAIM CRYPTONYM PREFIX");
                Console.WriteLine("4. CLAIM CRYPTONYM");
                Console.WriteLine(string.IsNullOrWhiteSpace(message) ? string.Empty : "> {0}", message);
                Console.WriteLine();
                Console.WriteLine("0. EXIT");

                if (!int.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out var num)) continue;
                Console.WriteLine("\n\n");
                switch (num)
                {
                    case 1:
                        ListCryptonymPrefixes(generator);
                        break;
                    case 2:
                        ListCryptonyms(generator);
                        break;
                    case 3:
                        message = await generator.ClaimPrefixAsync(PromptUntil("Enter a description: "));
                        break;
                    case 4:
                        message = await ClaimCryptonym(generator);
                        break;
                    case 0:
                        return;
                }
            }
        }
    }
}