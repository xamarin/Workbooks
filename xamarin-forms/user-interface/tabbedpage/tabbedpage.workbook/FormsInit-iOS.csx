#r "Xamarin.Forms.Core"
#r "Xamarin.Forms.Xaml"
#r "Xamarin.Forms.Platform"
#r "Xamarin.Forms.Platform.iOS"

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

class App : Application
{
    public App(){
        MainPage = new TabbedPage {
            Title = "TabbedWorkbook"
        };
    }
}

Xamarin.Forms.Forms.Init();
App app = new App();
KeyWindow.RootViewController = app.MainPage.CreateViewController();

TabbedPage page = app.MainPage as TabbedPage;
