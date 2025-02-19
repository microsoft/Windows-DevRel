using System;
using System.Collections.Generic;
using System.Linq;

namespace snowpal
{
    public class SnowpalGame
    {
        // Private properties
        private readonly List<string> _wordList = new List<string> { "windows", "view", "model", "taskbar", "xaml", "csharp", "debugger", "grid", "stackpanel", "random" };
        private const int MaxIncorrectGuessesValue = 6;
        private static readonly Random Random = new Random();

        // Public properties
        public string CurrentWord { get; private set; }
        public char[] GuessedWord { get; private set; }
        public int IncorrectGuesses { get; private set; }
        public int MaxIncorrectGuesses => MaxIncorrectGuessesValue;
        public int GuessesLeft => MaxIncorrectGuesses - IncorrectGuesses;

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

            for (int i = 0; i < CurrentWord.Length; i++)
            {
                if (CurrentWord[i] == letter)
                {
                    GuessedWord[i] = letter;
                    isCorrect = true;
                }
            }
            if (!isCorrect)
            {
                IncorrectGuesses++;
            }

            return isCorrect;
        }

        public string GetWordDisplay() => string.Join(" ", GuessedWord);

        public string GetHangmanImageSource() => $"ms-appx:///Assets/snow-{IncorrectGuesses}.png";

        public bool IsGameWon() => GuessedWord.All(c => c != '_');

        public bool IsGameOver() => IncorrectGuesses >= MaxIncorrectGuesses;

        public string GetWinningMessage(int GuessesLeft)
        {
            if (GuessesLeft == MaxIncorrectGuesses)
            {
                return "Incredible! You guessed the word without a single mistake! You're a true word master!";
            }
            else if (GuessesLeft == 1)
            {
                return "Phew! That was close! You guessed the word just in time! Well done!";
            }
            else
            {
                return "Great job! You guessed the word!";
            }

        }
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
