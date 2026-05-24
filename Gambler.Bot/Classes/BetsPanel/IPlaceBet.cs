using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Dice;
using System;

namespace Gambler.Bot.Classes.BetsPanel
{
    public interface iLiveBet
    {
        void AddBet(Bet newBet);
        event EventHandler<ViewBetEventArgs> BetClicked;

    }

    public interface iPlaceBet
    {
        public Games Game { get; }
        event EventHandler<PlaceBetEventArgs> PlaceBet;
        public IGameConfig GameSettings { get; set; }
        void BetCommand();
    }

    public interface iBetResult
    {

    }

    public class ViewBetEventArgs : EventArgs
    {
        public Bet BetToView { get; set; }
        public ViewBetEventArgs(Bet bettoview)
        {
            this.BetToView = bettoview;
        }
    }
    public class PlaceBetEventArgs : EventArgs
    {
        public PlaceBet NewBet { get; set; }
        public PlaceBetEventArgs(PlaceBet NewBet)
        {
            this.NewBet = NewBet;
        }
    }

}
