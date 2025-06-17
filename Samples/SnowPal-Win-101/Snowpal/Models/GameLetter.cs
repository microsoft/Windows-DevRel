using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowPal.Models
{
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
}
