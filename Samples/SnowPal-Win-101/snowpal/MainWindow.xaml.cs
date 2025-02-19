using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace snowpal
{
    public sealed partial class MainWindow : Window
    {
        // Public properties
        public SnowpalViewModel ViewModel { get; }

        // Constructor
        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new SnowpalViewModel();
            AlphabetButtonsListView.ItemsSource = GenerateAlphabet();
        }


        // Event handler for Grid Loaded event
        // TODO: is there way to handle this on the window instead? 
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            AlphabetButtonsListView.Focus(FocusState.Programmatic);
        }

        // Method to handle key up events
        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var key = e.Key.ToString().ToUpper();
            EnterGuess(key);
        }

        // Event handler for letter button clicks
        private void OnLetterButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var letter = button.Content.ToString().ToUpper();
                EnterGuess(letter);
            }
        }

        // Method to process the guessed letter
        private void EnterGuess(string letter)
        {
            var alphabet = (List<string>)AlphabetButtonsListView.ItemsSource;
            for (int i = 0; i < alphabet.Count; i++)
            {
                if (alphabet[i].ToUpper() == letter)
                {
                    var container = (ListViewItem)AlphabetButtonsListView.ContainerFromIndex(i);
                    var button = (Button)container.ContentTemplateRoot;
                    if (button != null && button.IsEnabled)
                    {
                        button.IsEnabled = false;
                        ViewModel.OnLetterGuessed(letter.ToLower()[0]);
                        CheckGameStatus();
                        break;
                    }
                }
            }
        }

        // Method to check the game status after each guess
        private async void CheckGameStatus()
        {
            if (ViewModel.IsGameWon())
            {
                // User has guessed the word correctly
                string title = "Congratulations!";
                string content = ViewModel.GetWinningMessage(ViewModel.GuessesLeft);

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
            var alphabet = (List<string>)AlphabetButtonsListView.ItemsSource;
            for (int i = 0; i < alphabet.Count; i++)
            {
                var container = (ListViewItem)AlphabetButtonsListView.ContainerFromIndex(i);
                var button = (Button)container.ContentTemplateRoot;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        // Method to generate the alphabet letters
        private List<string> GenerateAlphabet()
        {
            var alphabet = new List<string>();
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                alphabet.Add(letter.ToString());
            }
            return alphabet;
        }
    }
}
