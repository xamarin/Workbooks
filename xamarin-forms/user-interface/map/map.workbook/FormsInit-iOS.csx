#r "Xamarin.Forms.Core"
#r "Xamarin.Forms.Xaml"
#r "Xamarin.Forms.Platform"
#r "Xamarin.Forms.Platform.iOS"
#r "Xamarin.Forms.Maps"
#r "Xamarin.Forms.Maps.iOS"

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

class App : Application
{
    public App()
    {
        MainPage = new ContentPage();
    }
}

Xamarin.Forms.Forms.Init();
Xamarin.FormsMaps.Init();
App app = new App();
KeyWindow.RootViewController = app.MainPage.CreateViewController();

ContentPage page = app.MainPage as ContentPage;

StackLayout stackLayout = new StackLayout { Margin = new Thickness(0, 20, 0, 0) };
page.Content = stackLayout;
