using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Gaming.Input;

namespace SnowPal.Models
{
    public class SnowpalGame
    {
        // List of words for the game
        private readonly List<string> _wordList = new List<string> { "WINDOWS", "VIEW", "MODEL", "TASKBAR", "XAML", "CSHARP", "DEBUGGER", "GRID", "STACKPANEL", "RANDOM" };

        private const int MaxIncorrectGuesses = 6;

        // Messages for winning and losing the game
        private readonly string[] _winningMessages = {
                "Incredible! You guessed the word without a single mistake! You're a true word master!",
                "Phew! That was close! You guessed the word just in time! Well done!",
                "Great job! You guessed the word!"
            };
        private readonly string[] _losingMessages = {
                "Better luck next time! The word was {0}. Keep trying, you'll get it!",
                "Don't give up! The word was {0}.",
                "Oh no! The word was {0}.",
                "Sorry, you didn't guess it. The word was {0}.",
                "You ran out of guesses. The word was {0}."
            };

        // Properties for the current game state
        private const int MaxIncorrectGuessesValue = 6;

        public string CurrentWord { get; private set; }
        public char[] GuessedWord { get; private set; }
        public int IncorrectGuesses { get; private set; }
        public int GuessesLeft => MaxIncorrectGuesses - IncorrectGuesses;
        public bool GameEnd { get; private set; }
        public bool GameWon { get; private set; }
        public string MessageTitle { get; private set; }
        public string MessageContent { get; private set; }
                
        public SnowpalGame()
        {
            StartNewGame();
        }

        // Starts a new game by selecting a random word and resetting the game state
        public void StartNewGame()
        {
            var random = new Random();
            CurrentWord = _wordList[random.Next(_wordList.Count)];
            GuessedWord = new string('_', CurrentWord.Length).ToCharArray();
            IncorrectGuesses = 0;
            GameEnd = false;
            GameWon = false;
        }

        // Plays the game by guessing a letter and checking the game status
        public void PlayGame(char letter)
        {
            GuessLetter(letter);
            CheckGameStatus();
        }

        // Returns the current guessed word as a string with spaces between letters
        public string GetWordDisplay() => string.Join(" ", GuessedWord);


        // Guesses a letter and updates the guessed word and incorrect guesses count
        public void GuessLetter(char letter)
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
        }

        // Checks the game status to determine if the game is won or lost
        private void CheckGameStatus()
        {
            // User has guessed all the letters
            if (GuessedWord.All(c => c != '_'))
            {
                GameEnd = true;
                GameWon = true;
                MessageTitle = "Congratulations!";
                MessageContent = GetWinningMessage();
            }
            // User has run out of guesses
            else if (IncorrectGuesses >= MaxIncorrectGuesses)
            {
                GameEnd = true;
                MessageTitle = "Game Over!";
                MessageContent = GetLosingMessage();
            }
        }

        // Returns a winning message based on the number of guesses left
        private string GetWinningMessage()
        {
            return GuessesLeft switch
            {
                MaxIncorrectGuesses => _winningMessages[0],
                1 => _winningMessages[1],
                _ => _winningMessages[2]
            };
        }

        // Returns a random losing message
        private string GetLosingMessage()
        {
            var random = new Random();
            return string.Format(_losingMessages[random.Next(_losingMessages.Length)], CurrentWord);
        }

    }
}
