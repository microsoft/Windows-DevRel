using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace snowpal
{
    public sealed partial class MainWindow : Window
    {
        public SnowpalViewModel ViewModel { get; }

        // Constructor
        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new SnowpalViewModel();
        }

        // Event handler for Grid Loaded event
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            AlphabetButtonsGrid.Focus(FocusState.Programmatic);
        }

        // Method to handle key up events
        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var key = e.Key.ToString().ToUpper();
            foreach (var child in AlphabetButtonsGrid.Children)
            {
                if (child is Button button && button.Content.ToString().ToUpper() == key && button.IsEnabled)
                {
                    OnLetterButtonClick(button, new RoutedEventArgs());
                    break;
                }
            }
        }

        // Event handler for letter button clicks
        private void OnLetterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var letter = button.Content.ToString().ToLower()[0];
                button.IsEnabled = false;
                ViewModel.OnLetterGuessed(letter);
                CheckGameStatus();
            }
        }

        // Method to check the game status after each guess
        private async void CheckGameStatus()
        {
            if (ViewModel.IsGameWon())
            {
                // User has guessed the word correctly
                string title = "Congratulations!";
                string content;

                if (ViewModel.GuessesLeft == ViewModel.MaxIncorrectGuesses)
                {
                    content = "Incredible! You guessed the word without a single mistake! You're a true word master!";
                }
                else if (ViewModel.GuessesLeft >= 2 && ViewModel.GuessesLeft <= 5)
                {
                    content = "Great job! You guessed the word!";
                }
                else
                {
                    content = "Phew! That was close! You guessed the word just in time! Well done!";
                }

                MessageDialog.Title = title;
                MessageDialog.Content = content;
                await MessageDialog.ShowAsync();

                ViewModel.StartNewGame();
                ResetAlphabetButtons();
            }
            else if (ViewModel.IsGameOver())
            {
                // User has run out of guesses
                MessageDialog.Title = "Game Over";
                MessageDialog.Content = ViewModel.GetRandomGameOverMessage();
                await MessageDialog.ShowAsync();
                ViewModel.StartNewGame();
                ResetAlphabetButtons();
            }
        }

        // Method to reset the alphabet buttons for a new game
        private void ResetAlphabetButtons()
        {
            foreach (var child in AlphabetButtonsGrid.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = true;
                }
            }
        }
    }
}
