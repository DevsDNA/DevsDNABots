namespace WorkingWithWebview
{
    using Xamarin.Forms;

    public class WebPage : ContentPage
    {
        public WebPage()
        {
            var browser = new WebView();

            browser.Source = "https://webchat.botframework.com/embed/devsdna-web?s=cUEzxvLfI7k.cwA.c0k.sKypJZyqLk4MxdGAXQ84Sn-NP-Tyd-yBqJzhxy2A39A";

            this.Content = browser;
        }
    }
}