using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnowPal.Models;
using System;
using System.Collections.Generic;

namespace SnowPal;

public partial class SnowpalViewModel : ObservableObject
{
    private readonly Game _game;

    private string ImageSourcePath = "ms-appx:///Assets/snow-{0}.png";

    // Properties bound to the UI
    [ObservableProperty]
    public partial string WordDisplay { get; set; }

    [ObservableProperty]
    public partial List<GameLetter> Letters { get; set; }

    [ObservableProperty]
    public partial int IncorrectGuesses { get; set; }

    [ObservableProperty]
    public partial int GuessesLeft { get; set; }

    [ObservableProperty]
    public partial string MessageTitle { get; set; }

    [ObservableProperty]
    public partial string MessageContent { get; set; }
    [ObservableProperty]
    public partial string PopUpToDisplay { get; set; }
    [ObservableProperty]
    public partial string ImageSource { get; set; }


    // Constructor initializes the game and letters
    public SnowpalViewModel()
    {
        PopUpToDisplay = "false";
        Letters = new List<GameLetter>();
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            Letters.Add(new GameLetter(letter));
        }
        _game = new Game();
        StartNewGame();
    }

    // Starts a new game and updates the properties
    public void StartNewGame()
    {
        _game.StartNewGame();
        UpdateProperties();
    }

    // Command executed when a letter is guessed
    [RelayCommand]
    public void OnLetterGuessed(char LetterValue)
    {
        _game.PlayGame(LetterValue);
        if (_game.GameEnd)
        {
            EndGame();
        }
        else
        {
            UpdateProperties(LetterValue);
        }
    }

    // Ends the game, disables letters, and shows the end game message
    private void EndGame()
    {
        SetLettersIsEnabled(false);
        UpdateProperties();
        ShowEndGameMessage();

    }

    // Command executed when the popup close button is clicked
    [RelayCommand]
    private void ClosePopupClicked()
    {
        PopUpToDisplay = "false";
        SetLettersIsEnabled(true);
        StartNewGame();
    }

    // Shows the end game message in a popup
    private void ShowEndGameMessage()
    {
        MessageTitle = _game.MessageTitle;
        MessageContent = _game.MessageContent;
        PopUpToDisplay = "true";
    }

    // Updates the properties bound to the UI
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
        WordDisplay = string.Join(" ", _game.GuessedWord);
        IncorrectGuesses = _game.IncorrectGuesses;
        GuessesLeft = _game.GuessesLeft;
        ImageSource = string.Format(ImageSourcePath, IncorrectGuesses);
    }

    // Enables or disables the letter buttons
    private void SetLettersIsEnabled(bool status)
    {
        foreach (var letter in Letters)
        {
            letter.IsAvailable = status;
        }
    }
}
