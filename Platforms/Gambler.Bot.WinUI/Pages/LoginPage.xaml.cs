using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class LoginPage : Page
{
    private NavigationContext? _navigationContext;
    private LoginProfile? _profile;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        LoadProfile();
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_profile is null || _navigationContext is null)
        {
            ShowNoProfile();
            return;
        }

        var result = _navigationContext.LoginPreparationService.ValidateFields(_profile);
        LoginInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        LoginInfoBar.Title = result.Succeeded ? "Ready" : "Missing fields";
        LoginInfoBar.Message = result.Message;
    }

    private void SimulationButton_Click(object sender, RoutedEventArgs e)
    {
        if (_profile is null || _navigationContext is null)
        {
            ShowNoProfile();
            return;
        }

        _navigationContext.SiteSessionService.StartSimulation(_profile.Site);
        LoginInfoBar.Severity = InfoBarSeverity.Success;
        LoginInfoBar.Title = "Simulation active";
        LoginInfoBar.Message = $"{_profile.Site.Name} is active in simulation mode.";
    }

    private async void LiveLoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_profile is null || _navigationContext is null)
        {
            ShowNoProfile();
            return;
        }

        LoginCommandBar.IsEnabled = false;
        LoginInfoBar.Severity = InfoBarSeverity.Informational;
        LoginInfoBar.Title = "Logging in";
        LoginInfoBar.Message = $"Attempting login for {_profile.Site.Name}.";

        var result = await _navigationContext.LiveLoginService.LoginAsync(_profile);
        LoginInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        LoginInfoBar.Title = result.Succeeded ? "Login complete" : "Login failed";
        LoginInfoBar.Message = result.Message;
        LoginCommandBar.IsEnabled = true;

        RenderLoginFields(_profile);
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { Tag: LoginFieldModel field } textBox)
        {
            field.Value = textBox.Text;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox { Tag: LoginFieldModel field } passwordBox)
        {
            field.Value = passwordBox.Password;
        }
    }

    private void LoadProfile()
    {
        var site = _navigationContext?.SiteSessionService.Current.SelectedSite;
        if (site is null)
        {
            ShowNoProfile();
            return;
        }

        _profile = _navigationContext?.LoginPreparationService.GetProfile(site);
        if (_profile is null)
        {
            ShowNoProfile();
            return;
        }

        LoginSubtitleText.Text = $"Preparing login for {_profile.Site.Name}.";
        RenderLoginFields(_profile);
        NormalLoginText.Text = _profile.SupportsNormalLogin ? "Normal login available" : "Normal login unavailable";
        BrowserLoginText.Text = _profile.SupportsBrowserLogin ? "Browser login available" : "Browser login unavailable";
        MirrorText.Text = $"Mirrors: {string.Join(", ", _profile.Mirrors)}";
        LoginInfoBar.Severity = InfoBarSeverity.Informational;
        LoginInfoBar.Title = "Login fields loaded";
        LoginInfoBar.Message = $"{_profile.Fields.Count} fields loaded from Core site metadata.";
    }

    private void RenderLoginFields(LoginProfile profile)
    {
        LoginFieldsPanel.Children.Clear();

        foreach (var field in profile.Fields)
        {
            var label = new TextBlock
            {
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Text = field.IsRequired ? $"{field.Name} *" : field.Name
            };

            var stack = new StackPanel
            {
                Spacing = 4
            };

            stack.Children.Add(label);

            if (field.IsSecret || field.IsMfa)
            {
                var passwordBox = new PasswordBox
                {
                    PlaceholderText = field.IsMfa ? "MFA code" : "Secret value",
                    Password = field.Value ?? string.Empty,
                    Tag = field,
                    MaxWidth = 480,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                stack.Children.Add(passwordBox);
            }
            else
            {
                var textBox = new TextBox
                {
                    PlaceholderText = "Value",
                    Text = field.Value ?? string.Empty,
                    Tag = field,
                    MaxWidth = 480,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                textBox.TextChanged += TextBox_TextChanged;
                stack.Children.Add(textBox);
            }

            LoginFieldsPanel.Children.Add(stack);
        }
    }

    private void ShowNoProfile()
    {
        LoginSubtitleText.Text = "Select a site before preparing login.";
        LoginFieldsPanel.Children.Clear();
        MirrorText.Text = "";
        LoginInfoBar.Severity = InfoBarSeverity.Warning;
        LoginInfoBar.Title = "No active site";
        LoginInfoBar.Message = "Select a site on the Sites page first.";
    }
}
