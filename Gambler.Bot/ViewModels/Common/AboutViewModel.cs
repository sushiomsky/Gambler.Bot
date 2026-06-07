using ActiproSoftware.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gambler.Bot.ViewModels.Common
{
    public  class AboutViewModel:ViewModelBase
    {
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string License { get; set; }
        public string Copyright { get; set; }
        public string ContactEmail { get; set; }
        public string TelegramHandle { get; set; }
        public string Acknowledgements { get; set; }
        public AboutViewModel(ILogger logger):base(logger)
        {
            Version = App.GetVersion();
            Author = "Seuntjie";
            Description = "A bot for gambling sites";
            License = "MIT";
            Copyright = "© 2024 Botma Software (Pty) ltd - All rights reserved";
            ContactEmail = "schnickfitzel1@gmail.com";
            TelegramHandle = "@yzymowep";
            Acknowledgements = "Thanks to all the people who helped me with this project";
        }
    }
}
