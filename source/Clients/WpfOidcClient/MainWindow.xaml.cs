using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using WpfOidcClient.OidcClient;

namespace WpfOidcClient
{
    public partial class MainWindow : Window
    {
        LoginWebView _login;

        public MainWindow()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
            //IdentityTextBox.Visibility = Visibility.Hidden;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //var settings = new OidcSettings
            //{
            //    Authority = "https://localhost:44333/core",
            //    ClientId = "wpf.hybrid",
            //    ClientSecret = "secret",
            //    RedirectUri = "http://localhost/wpf.hybrid",
            //    Scope = "openid profile write",
            //    LoadUserProfile = true
            //};

            _oidcSettings = new OidcSettings
            {
                Authority = "https://idgatewayawsstage.flqa.net",
                ClientId = "nativeClientPOC",
                ClientSecret = "secret",
                RedirectUri = "http://localhost/wpf.hybrid",
                Scope = "openid offline_access flapi.public profile",
                LoadUserProfile = true
            };

            await _login.LoginAsync(_oidcSettings);
        }

        void _login_Done(object sender, LoginResult e)
        {
            _loginResult = e;

            if (_loginResult.Success)
            {
                var sb = new StringBuilder(128);

                foreach (var claim in _loginResult.User.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                sb.AppendLine();

                sb.AppendLine($"Identity token: {_loginResult.IdentityToken}");
                sb.AppendLine();
                sb.AppendLine($"Access token: {_loginResult.AccessToken}");
                sb.AppendLine($"Access token expiration: {_loginResult.AccessTokenExpiration}");
                sb.AppendLine();
                sb.AppendLine($"Refresh token: {_loginResult?.RefreshToken ?? "none" }");

                IdentityTextBox.Text = sb.ToString();
            }
            else
            {
                IdentityTextBox.Text = _loginResult.ErrorMessage;
            }
        }

        private LoginResult _loginResult;
        private OidcSettings _oidcSettings;

        private static async Task<TokenResponse> RefreshToken(OidcSettings settings, string refreshToken)
        {
            var discoAddress = settings.Authority + "/.well-known/openid-configuration";

            var manager = new ConfigurationManager<OpenIdConnectConfiguration>(discoAddress);

            var config = await manager.GetConfigurationAsync();

            var tokenClient = new TokenClient(
                config.TokenEndpoint,
                settings.ClientId,
                settings.ClientSecret);

            return await tokenClient.RequestRefreshTokenAsync(refreshToken);
        }

        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            var response = await RefreshToken(_oidcSettings, _loginResult.RefreshToken);

            IdentityTextBox.Clear();

            if (!response.IsHttpError)
            {
                _loginResult.AccessToken = response.AccessToken;
                _loginResult.AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn);
                _loginResult.RefreshToken = response.RefreshToken;

                var sb = new StringBuilder(128);

                foreach (var claim in _loginResult.User.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                sb.AppendLine();

                sb.AppendLine($"Identity token: {_loginResult.IdentityToken}");
                sb.AppendLine();
                sb.AppendLine($"Access token: {_loginResult.AccessToken}");
                sb.AppendLine($"Access token expiration: {_loginResult.AccessTokenExpiration}");
                sb.AppendLine();
                sb.AppendLine($"Refresh token: {_loginResult?.RefreshToken ?? "none" }");

                IdentityTextBox.Text = sb.ToString();
            }
            else
            {
                IdentityTextBox.Text = response.HttpErrorStatusCode + " " + response.HttpErrorReason;
            }
        }
    }
}