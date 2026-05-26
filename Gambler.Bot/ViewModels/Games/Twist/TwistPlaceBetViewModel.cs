using Gambler.Bot.Classes.BetsPanel;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.ViewModels.Games.Dice;
using Microsoft.Extensions.Logging;
using System;

namespace Gambler.Bot.ViewModels.Games.Twist
{
    public class TwistPlaceBetViewModel :DicePlaceBetViewModel
    {
        public override event EventHandler<PlaceBetEventArgs> PlaceBet;
        public TwistPlaceBetViewModel(ILogger logger) : base(logger)
        {
        }
        public override Bot.Common.Games.Games Game => Bot.Common.Games.Games.Twist;

        protected override void Bet(bool High)
        {
            PlaceBet?.Invoke(this, new PlaceBetEventArgs(new PlaceTwistBet(Amount,HighChecked, Payout)));
        }
    }
}
