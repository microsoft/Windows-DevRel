using SnowPal.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace SnowPal
{
    public class SnowpalViewModel : INotifyPropertyChanged
    {
        private readonly SnowpalGame _game;

        private string wordDisplay;
        public string WordDisplay
        {
            get => wordDisplay;
            set
            {
                if (wordDisplay != value)
                {
                    wordDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        private int incorrectGuesses;
        public int IncorrectGuesses
        {
            get => incorrectGuesses;
            set
            {
                if (incorrectGuesses != value)
                {
                    incorrectGuesses = value;
                    OnPropertyChanged();
                }
            }
        }

        private int guessesLeft;
        public int GuessesLeft
        {
            get => guessesLeft;
            set
            {
                if (guessesLeft != value)
                {
                    guessesLeft = value;
                    OnPropertyChanged();
                }
            }
        }

        public SnowpalViewModel()
        {
            _game = new SnowpalGame();
            StartNewGame();
        }

        public void StartNewGame()
        {
            _game.StartNewGame();
            UpdateProperties();
        }

        public void OnLetterGuessed(char letter)
        {
            _game.GuessLetter(letter);
            UpdateProperties();
        }

        public bool IsGameWon() => _game.IsGameWon();
        public bool IsGameOver() => _game.IsGameOver();
        public string GetWinningMessage(int guessesLeft) => _game.GetWinningMessage(guessesLeft);
        public string GetRandomGameOverMessage() => _game.GetRandomGameOverMessage();

        private void UpdateProperties()
        {
            WordDisplay = _game.GetWordDisplay();
            IncorrectGuesses = _game.IncorrectGuesses;
            GuessesLeft = _game.GuessesLeft;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}