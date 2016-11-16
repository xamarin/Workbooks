public class TabController : UITabBarController{
    public UIViewController safariTab, webViewTab, WKWebTab, safariVCTab;

    public TabController (){

        safariTab = new UIViewController(){Title = "Safari"};
        safariTab.View.BackgroundColor = UIColor.White;

        webViewTab = new UIViewController(){Title = "UIWebView"};

        WKWebTab = new UIViewController(){Title="WKWebView"};

        safariVCTab = new UIViewController(){Title="SFSafariViewController"};
        safariVCTab.View.BackgroundColor = UIColor.White;
    
    var tabs = new UIViewController[]{webViewTab, WKWebTab, safariVCTab, safariTab};

    ViewControllers = tabs;
    }
}

var tabController = new TabController();
KeyWindow.RootViewController = tabController;