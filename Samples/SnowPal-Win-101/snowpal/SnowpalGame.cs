using System;
using System.Collections.Generic;
using System.Linq;

namespace snowpal
{
    public class SnowpalGame
    {
        private readonly List<string> _wordList = new List<string> { "apple", "banana", "cherry", "date", "fig", "grape" };
        private const int MaxIncorrectGuessesValue = 6;
        private static readonly Random Random = new Random();

        public string CurrentWord { get; private set; }
        public char[] GuessedWord { get; private set; }
        public int IncorrectGuesses { get; private set; }
        public int MaxIncorrectGuesses => MaxIncorrectGuessesValue;

        public SnowpalGame()
        {
            StartNewGame();
        }

        public void StartNewGame()
        {
            var random = new Random();
            CurrentWord = _wordList[random.Next(_wordList.Count)];
            GuessedWord = new string('_', CurrentWord.Length).ToCharArray();
            IncorrectGuesses = 0;
        }

        public bool GuessLetter(char letter)
        {
            bool isCorrect = false;

            if (CurrentWord.Contains(letter))
            {
                for (int i = 0; i < CurrentWord.Length; i++)
                {
                    if (CurrentWord[i] == letter)
                    {
                        GuessedWord[i] = letter;
                        isCorrect = true;
                    }
                }
            }
            else
            {
                IncorrectGuesses++;
            }

            return isCorrect;
        }

        public int GuessesLeft => MaxIncorrectGuesses - IncorrectGuesses;

        public string GetWordDisplay() => string.Join(" ", GuessedWord);

        public string GetHangmanImageSource() => $"ms-appx:///Assets/snow-{IncorrectGuesses}.png";

        public bool IsGameWon() => GuessedWord.All(c => c != '_');

        public bool IsGameOver() => IncorrectGuesses >= MaxIncorrectGuesses;

        public string GetRandomGameOverMessage()
        {
            var messages = new[]
            {
                    $"Game Over! Better luck next time! The word was {CurrentWord}. Keep trying, you'll get it!",
                    $"Game Over! Don't give up! The word was {CurrentWord}. Practice makes perfect!",
                    $"Game Over! The word was {CurrentWord}. Remember, every mistake is a step towards success!"
                };

            return messages[Random.Next(messages.Length)];
        }
    }
}
