using UIKit;

public class ContentView : UIView
{
    public ContentView (UIColor fillColor)
    {
        BackgroundColor = fillColor;
    }
}

public class SimpleViewController : UIViewController
{
    UIColor fillColor; 

    public SimpleViewController (UIColor fillColor) : base ()
    {
        this.fillColor = fillColor;
    }

    public override void DidReceiveMemoryWarning ()
    {
        // Releases the view if it doesn't have a superview.
        base.DidReceiveMemoryWarning ();
    }

    public override void ViewDidLoad ()
    {
        base.ViewDidLoad ();

        var view = new ContentView (fillColor);

        this.View = view;
    }
}

var cyanController = new SimpleViewController(UIColor.Cyan);
var blueController = new SimpleViewController(UIColor.Blue);
var redController = new SimpleViewController(UIColor.Red);
var greenController = new SimpleViewController(UIColor.Green);