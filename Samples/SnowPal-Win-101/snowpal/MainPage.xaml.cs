using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SnowPal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using SnowPal.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SnowPal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public SnowpalViewModel ViewModel { get; } = new();

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new SnowpalViewModel();
            this.DataContext = ViewModel;
            AlphabetButtonsGridView.ItemsSource = GenerateAlphabet();
        }

        // Event handler for Grid Loaded event
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            AlphabetButtonsGridView.Focus(FocusState.Programmatic);
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
            var alphabet = (List<string>)AlphabetButtonsGridView.ItemsSource;
            for (int i = 0; i < alphabet.Count; i++)
            {
                if (alphabet[i].ToUpper() == letter)
                {
                    var container = (GridViewItem)AlphabetButtonsGridView.ContainerFromIndex(i);
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
            var alphabet = (List<string>)AlphabetButtonsGridView.ItemsSource;
            for (int i = 0; i < alphabet.Count; i++)
            {
                var container = (GridViewItem)AlphabetButtonsGridView.ContainerFromIndex(i);
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
