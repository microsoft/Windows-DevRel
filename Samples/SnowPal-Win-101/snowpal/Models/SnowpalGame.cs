using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Gaming.Input;

namespace SnowPal.Models
{
    public class SnowpalGame
    {
        private readonly List<string> _wordList = ["WINDOWS", "VIEW", "MODEL", "TASKBAR", "XAML", "CSHARP", "DEBUGGER", "GRID", "STACKPANEL", "RANDOM"];
        private const int MaxIncorrectGuessesValue = 6;

        public string CurrentWord { get; private set; }
        public char[] GuessedWord { get; private set; }
        public int IncorrectGuesses { get; private set; }
        public int MaxIncorrectGuesses => MaxIncorrectGuessesValue;
        public int GuessesLeft => MaxIncorrectGuesses - IncorrectGuesses;
        public bool GameEnd = false;
        public bool GameWon = false;

        public string message { get; private set; }

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
            GameWon = false;
        }

        public void PlayGame(char letter)
        {
            GuessLetter(letter);
            CheckGameStatus();

        }

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

        public string GetWordDisplay() => string.Join(" ", GuessedWord);

        public void CheckGameStatus()
        {
            if (GuessedWord.All(c => c != '_'))
            {
                Debug.WriteLine("Game won");
                GameEnd = true;
                GameWon = true;
                StartNewGame();
            }
            else if (IncorrectGuesses >= MaxIncorrectGuesses)
            {
                Debug.WriteLine("Game over");
                GameEnd = true;
                StartNewGame();
            }
        }

        public string GetWinningMessage()
        {
            return "Congratulations! You guessed the word!";
        }

        public string GetLosingMessage()
        {
            return $"Game Over! The word was {CurrentWord}. Better luck next time!";
        }




        //public string GetWinningMessage(int GuessesLeft)
        //{
        //    if (GuessesLeft == MaxIncorrectGuesses)
        //    {
        //        return "Incredible! You guessed the word without a single mistake! You're a true word master!";
        //    }
        //    else if (GuessesLeft == 1)
        //    {
        //        return "Phew! That was close! You guessed the word just in time! Well done!";
        //    }
        //    else
        //    {
        //        return "Great job! You guessed the word!";
        //    }

        //}
        //public string GetRandomGameOverMessage()
        //{
        //    var messages = new[]
        //    {
        //                $"Game Over! Better luck next time! The word was {CurrentWord}. Keep trying, you'll get it!",
        //                $"Game Over! Don't give up! The word was {CurrentWord}. Practice makes perfect!",
        //                $"Game Over! The word was {CurrentWord}. Remember, every mistake is a step towards success!"
        //            };

        //    return messages[Random.Next(messages.Length)];
        //}
    }
}
