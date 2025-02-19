using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace snowpal
{
    public class SnowpalViewModel : INotifyPropertyChanged
    {
        private readonly SnowpalGame _game;

        public SnowpalViewModel()
        {
            _game = new SnowpalGame();
            OnPropertyChanged(nameof(WordDisplay));
            OnPropertyChanged(nameof(HangmanImageSource));
            OnPropertyChanged(nameof(GuessesLeft));
        }

        public string WordDisplay => _game.GetWordDisplay();

        public int IncorrectGuesses => _game.IncorrectGuesses;

        public string CurrentWord => _game.CurrentWord;

        public int GuessesLeft => _game.GuessesLeft;

        public string HangmanImageSource => _game.GetHangmanImageSource();

        public int MaxIncorrectGuesses => _game.MaxIncorrectGuesses;

        public void StartNewGame()
        {
            _game.StartNewGame();
            OnPropertyChanged(nameof(WordDisplay));
            OnPropertyChanged(nameof(HangmanImageSource));
            OnPropertyChanged(nameof(GuessesLeft));
        }

        public void OnLetterGuessed(char letter)
        {
            _game.GuessLetter(letter);
            OnPropertyChanged(nameof(WordDisplay));
            OnPropertyChanged(nameof(HangmanImageSource));
            OnPropertyChanged(nameof(GuessesLeft));
        }

        public bool IsGameWon() => _game.IsGameWon();

        public bool IsGameOver() => _game.IsGameOver();

        public string GetRandomGameOverMessage() => _game.GetRandomGameOverMessage();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}