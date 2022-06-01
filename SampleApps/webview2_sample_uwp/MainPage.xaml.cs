// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

 #pragma warning disable CS8305 //'Microsoft.UI.Xaml.Controls.WebView2' is for evaluation purposes only and is subject to change or removal in future updates.

using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace webview2_sample_uwp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            AddressBar.Text = "https://developer.microsoft.com/en-us/microsoft-edge/webview2/";
            WebView2.Source = new Uri(AddressBar.Text);

            WebView2.NavigationCompleted += WebView2_NavigationCompleted;

            StatusUpdate("Ready");

            InitializeCoreWebView2Async();
        }

        public async Task<CoreWebView2> EnsureCoreWebView2Async()
        {
            await WebView2.EnsureCoreWebView2Async();
            return WebView2.CoreWebView2;
        }

        private async void InitializeCoreWebView2Async()
        {
            await WebView2.EnsureCoreWebView2Async();
            WebView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private async  void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            Console.WriteLine("NewWindowRequested");

            switch (NewWindowBehaviorComboBox.SelectedIndex)
            {
                // Open mini browser window
                case 0:
                    // Default behavior. No change to args required.
                    break;

                // Send to default browser 
                case 1:
                    args.Handled = true;
                    // No need to wait for the launcher to finish sending the URI to the browser
                    // before we allow the WebView2 in our app to continue.
                    _ = Windows.System.Launcher.LaunchUriAsync(new Uri(args.Uri));
                    break;

                // Navigate in same WebView2
                case 2:
                    args.Handled = true;
                    args.NewWindow = sender;
                    break;

                // Open in different WebView2
                case 3:
                    args.Handled = true;
                    using (args.GetDeferral())
                    {
                        await WebView2Right.EnsureCoreWebView2Async();
                        args.NewWindow = WebView2Right.CoreWebView2;
                    }
                    break;
            }
        }

        private void StatusUpdate(string message)
        {
            StatusBar.Text = message;
            Debug.WriteLine(message);
        }

        private void WebView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            StatusUpdate("Navigation complete");
        }

        private bool TryCreateUri(String potentialUri, out Uri result)
        {
            StatusUpdate("TryCreateUri");

            Uri uri;
            if ((Uri.TryCreate(potentialUri, UriKind.Absolute, out uri) || Uri.TryCreate("http://" + potentialUri, UriKind.Absolute, out uri)) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                result = uri;
                return true;
            }
            else
            {
                StatusUpdate("Unable to configure URI");
                result = null;
                return false;
            }
        }

        private void TryNavigate()
        {
            StatusUpdate("TryNavigate");

            Uri destinationUri;
            if (TryCreateUri(AddressBar.Text, out destinationUri))
            {
                WebView2.Source = destinationUri;
            }
            else
            {
                StatusUpdate("URI couldn't be figured out use it as a bing search term");

                String bingString = "https://www.bing.com/search?q=" + Uri.EscapeUriString(AddressBar.Text);
                if (TryCreateUri(bingString, out destinationUri))
                {
                    AddressBar.Text = destinationUri.AbsoluteUri;
                    WebView2.Source = destinationUri;
                }
                else
                {
                    StatusUpdate("URI couldn't be configured as bing search term, giving up");
                }
            }
        }

        private void Go_OnClick(object sender, RoutedEventArgs e)
        {
            StatusUpdate("Go_OnClick: " + AddressBar.Text);

            TryNavigate();
        }

        private void AddressBar_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                StatusUpdate("AddressBar_KeyDown [Enter]: " + AddressBar.Text);

                e.Handled = true;
                TryNavigate();
            }
        }
    }
}
