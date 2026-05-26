using Gambler.Bot.Classes.BetsPanel;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.ViewModels.Games.Dice;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace Gambler.Bot.ViewModels.Games.Limbo
{
    public class LimboPlaceBetViewModel :DicePlaceBetViewModel
    {
        public override event EventHandler<PlaceBetEventArgs> PlaceBet;
        public LimboPlaceBetViewModel(ILogger logger) : base(logger)
        {
        }
        public override Bot.Common.Games.Games Game => Bot.Common.Games.Games.Limbo;
        protected override void Bet(bool High)
        {
            PlaceBet?.Invoke(this, new PlaceBetEventArgs(new PlaceLimboBet(Amount, Payout)));
        }
    }
}
