# Cryptonym Generator

A command line app for creating [CIA style cryptonyms](https://en.wikipedia.org/wiki/CIA_cryptonym) or code words (like the infamous MKULTRA) using dictionary web APIs. Use this tool if you need several labels or project names that are:
- good enough for real life late 20th century intelligence operations
- perfectly opaque yet slightly ominous
- namespaced by their first 2 letters so related code words are sorted together   


## Data Source

The current implementation uses the Global series on the Lexicala API. It can pull down a decent amount of nouns and a free account is enough. It also supports [a lot of different languages](https://api.lexicala.com/documentation) so you can probably generate cryponyms in your local langauge as long as it uses the Latin alphabet.

Going forward the next step is to introduce the Password series from the same API and after that even more APIs for better coverage. If you have any suggestions please open an issue (or a pull request if you want to do it yourself).


## Requirements

This program requires [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) to run. You need internet connection only for the first usage to download the dictionary file.


## Usage

The interactive command line tool can be used like this:

```powershell
dotnet CryptonymGenerator.dll [Lexicala_user [Lexicala_pass [two_letter_language_code [part_of_speech]]]]
```

- Lexicala user & pass are your account credentials on lexicala.com. If no arguments are passed it is prompted in command line.
- The two letter language code is an [ISO 639-1](https://en.wikipedia.org/wiki/ISO_639-1) code, eg. `en`, `de` or `hu`. If the argument is omitted it defaults to English. 
- The [part of speech](https://en.wikipedia.org/wiki/Part_of_speech) is the type of word to filter by when possible. Defaults to `noun`.


## Example

This is an example output of the very first couple test entries it has generated with the Global English Lexicala data source: (any perceived undertone is coincidental but also a proof of the tool's effective "mood")

```
1. LIST CRYPTONYM PREFIXES
2. LIST CRYPTONYMS
3. CLAIM CRYPTONYM PREFIX
4. CLAIM CRYPTONYM


0. EXIT
   2



CRYPTONYMS
**********

WOCLIMB: TEST PERSON OF INTEREST (TEST)
WOBANDWAGON: TEST PROJECT (TEST)
MIUNCONSCIOUS: TARGET (ENEMY)
MIWORKINGCLASS: UNKNOWN PROJECT (ENEMY)
Press Enter to continue...
```
