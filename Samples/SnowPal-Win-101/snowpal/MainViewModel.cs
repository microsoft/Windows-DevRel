using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SnowPal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Sms;
using Windows.UI.Popups;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace SnowPal;

public partial class SnowpalViewModel : ObservableObject
{
    private readonly SnowpalGame _game;

    [ObservableProperty]
    public partial string WordDisplay { get; set; }

    [ObservableProperty]
    public partial int IncorrectGuesses { get; set; }

    [ObservableProperty]
    public partial int GuessesLeft { get; set; }

    [ObservableProperty]
    public partial List<GameLetter> Letters { get; set; }

    [ObservableProperty]
    public partial string ImageSource { get; set; }

    // MessageTitle, MessageContent & EndGameMessageRequested were two ways I was trying to make the ContentDialog work
    [ObservableProperty]
    public partial string MessageTitle { get; set; }

    [ObservableProperty]
    public partial string MessageContent { get; set; }

    public event Action EndGameMessageRequested;

    public SnowpalViewModel()
    {
        Letters = new List<GameLetter>();
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            Letters.Add(new GameLetter(letter));
        }
        _game = new SnowpalGame();
        StartNewGame();
    }

    public void StartNewGame()
    {
        _game.StartNewGame();
        UpdateProperties();
    }

    [RelayCommand]
    public void OnLetterGuessed(char LetterValue)
    {
        _game.PlayGame(LetterValue);
        if (_game.GameEnd == true)
        {
            EndGame();
        }
        else
        {
            UpdateProperties(LetterValue);
        }
    }

    private void EndGame()
    {
        ResetLetters();
        UpdateProperties();
        ShowEndGameMessage();
        _game.GameEnd = false;
    }

    private async void ShowEndGameMessage()
    {
        EndGameMessageRequested?.Invoke();

        //var dialog = new ContentDialog
        //{
        //    Title = _game.GameWon ? "Congratulations!" : "Game Over",
        //    Content = _game.GameWon ? _game.GetWinningMessage() : _game.GetLosingMessage(),
        //    CloseButtonText = "OK"
        //};

        //await dialog.ShowAsync();

    }

    private void UpdateProperties(char LetterValue = '\0')
    {
        if (LetterValue != '\0')
        {
            GameLetter foundLetter = Letters.Find(letter => letter.Character == LetterValue);
            if (foundLetter != null)
            {
                foundLetter.IsAvailable = false;
            }
        }
        WordDisplay = _game.GetWordDisplay();
        IncorrectGuesses = _game.IncorrectGuesses;
        GuessesLeft = _game.GuessesLeft;
        ImageSource = $"ms-appx:///Assets/snow-{IncorrectGuesses}.png";
    }

    public void ResetLetters()
    {
        foreach (var letter in Letters)
        {
            letter.IsAvailable = true;
        }
    }
}
