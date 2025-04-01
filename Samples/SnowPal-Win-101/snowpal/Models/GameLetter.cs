using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace SnowPal.Models;

public partial class GameLetter : ObservableObject
{
    public char Character { get; set; }

    [ObservableProperty]
    public partial bool IsAvailable { get; set; }

    public GameLetter(char character)
    {
        this.Character = character;
        this.IsAvailable = true;
    }

}
