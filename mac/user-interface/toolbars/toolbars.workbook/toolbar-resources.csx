using System;
using Foundation;
using AppKit;
using CoreGraphics;


// ==========================================================================
// NOTE: This file contains the backing resources to create the Windows,
// View Controllers and View needed to support the Working with Toolbars
// Workbook. The implementation of these controllers is not important to 
// understanding the `NSToolBar` control so they have been moved here.
// ==========================================================================



/// <summary>
/// The LayerBackedView class is used to create a modern View within a 
/// View Controller that is backed by a Core Graphics Layer.
/// </summary>
public class LayerBackedView : NSView
{
	#region Private Variables
	/// <summary>
	/// The color of the background.
	/// </summary>
	private NSColor _backgroundColor = NSColor.White;
	#endregion

	#region Computed Properties
	/// <summary>
	/// Gets a value indicating whether this <see cref="T:LayerBackedView"/> wants layer.
	/// </summary>
	/// <value><c>true</c> if wants layer; otherwise, <c>false</c>.</value>
	public override bool WantsLayer {
		get { return true; }
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="T:LayerBackedView"/> wants to update 
	/// the layer.
	/// </summary>
	/// <value><c>true</c> if wants update layer; otherwise, <c>false</c>.</value>
	public override bool WantsUpdateLayer {
		get { return true; }
	}

	/// <summary>
	/// Gets or sets the color of the background.
	/// </summary>
	/// <value>The color of the background.</value>
	public NSColor BackgroundColor {
		get { return _backgroundColor; }
		set {
			// Save color
			_backgroundColor = value;

			// Force the view to update
			NeedsDisplay = true;
		}
	}
	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of the <see cref="T:LayerBackedView"/> class.
	/// </summary>
	/// <param name="handle">Handle.</param>
	public LayerBackedView (IntPtr handle) : base (handle)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:LayerBackedView"/> class.
	/// </summary>
	/// <param name="bounds">Bounds.</param>
	public LayerBackedView (CGRect bounds) : base (bounds)
	{
	}
	#endregion

	#region Override Methods
	/// <summary>
	/// Updates the layer.
	/// </summary>
	public override void UpdateLayer ()
	{
		base.UpdateLayer ();

		// Add the Core Graphics routines to draw the View's UI
		Layer.BackgroundColor = _backgroundColor.CGColor;
	}
	#endregion
}

/// <summary>
/// Traditional code based window that uses the traditional look and feel
/// of an OS X Window. Typically this Window would be created in a Xcode
/// Interface Builder `.storyboard` file. For the sake of this workbook,
/// the Window is being created in code.
/// </summary>
public class TraditionalCodeBasedWindow : NSWindow
{

	#region Computed Properties
	/// <summary>
	/// Gets or sets the click me label.
	/// </summary>
	/// <value>The click me label.</value>
	public NSTextField ClickMeLabel { get; set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="handle">Handle.</param>
	public TraditionalCodeBasedWindow (IntPtr handle) : base (handle)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="coder">Coder.</param>
	[Export ("initWithCoder:")]
	public TraditionalCodeBasedWindow (NSCoder coder) : base (coder)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="contentRect">Content rect.</param>
	/// <param name="aStyle">A style.</param>
	/// <param name="bufferingType">Buffering type.</param>
	/// <param name="deferCreation">If set to <c>true</c> defer creation.</param>
	public TraditionalCodeBasedWindow (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base (contentRect, aStyle, bufferingType, deferCreation)
	{
		// Create a default title for the window
		Title = "Untitled";

		// Create the content view for the window and make it fill the window
		ContentView = new LayerBackedView (Frame);

		// _________________________________________________________________
        // WARNING! 
        // The modern macOS App UI is currently unavaiable in the Version 
        // 0.99.0.0 of Xamarin Workbooks. This feature will be restored in a 
        // future release.
        // _________________________________________________________________
		// Configure the Window to use Tabs
		// TabbingMode = NSWindowTabbingMode.Preferred;
		// TabbingIdentifier = "Traditional";

		// Define Window UI
		ClickMeLabel = new NSTextField (new CGRect (10, Frame.Height - 125, Frame.Width - 20, 20)) {
			BackgroundColor = NSColor.Clear,
			TextColor = NSColor.LabelColor,
			Editable = false,
			Bezeled = false,
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.MinYMargin,
			StringValue = "Click on a Toolbar item above."
		};
		ContentView.AddSubview (ClickMeLabel);
	}
	#endregion

	#region Override Method
	/// <summary>
	/// Awakes from nib.
	/// </summary>
	public override void AwakeFromNib ()
	{
		base.AwakeFromNib ();

		// Build and configure a new toolbar
		var toolbar = new MainToolbar ("TraditionalToolbar");

		// Wireup toolbar events
		toolbar.ToolbarDelegate.ToolbarItemClicked += (identifier) => {
			ClickMeLabel.StringValue = $"You clicked the {identifier} Toolbar Item.";
		};

		// Attach toolbar to window
		this.Toolbar = toolbar;
	}
	#endregion
}

/// <summary>
/// Traditional code based Window Controller instantiats and controls an instance of
/// the `TraditionalCodeBasedWindow` defined above.
/// </summary>
public class TraditionalCodeBasedWindowController : NSWindowController
{
	#region Computed Properties
	/// <summary>
	/// Gets the window.
	/// </summary>
	/// <value>The window.</value>
	public new TraditionalCodeBasedWindow Window {
		get { return base.Window as TraditionalCodeBasedWindow; }
	}
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindowController"/> class.
	/// </summary>
	/// <param name="handle">Handle.</param>
	public TraditionalCodeBasedWindowController (IntPtr handle) : base (handle)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindowController"/> class.
	/// </summary>
	/// <param name="coder">Coder.</param>
	[Export ("initWithCoder:")]
	public TraditionalCodeBasedWindowController (NSCoder coder) : base (coder)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:TraditionalCodeBasedWindowController"/> class.
	/// </summary>
	public TraditionalCodeBasedWindowController () : base ("CodeBasedWindow")
	{
		// Define the Window's default location and size
		CGRect contentRect = new CGRect (0, 0, 600, 500);

		// Create a new instance of the CodeBasedWindow
		base.Window = new TraditionalCodeBasedWindow (contentRect, (NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.FullSizeContentView), NSBackingStore.Buffered, false);

		// Simulate Awaking from Nib
		Window.AwakeFromNib ();
	}
	#endregion

	#region Override Methods
	/// <summary>
	/// Awakes from nib.
	/// </summary>
	public override void AwakeFromNib ()
	{
		base.AwakeFromNib ();
	}

	// _________________________________________________________________
    // WARNING! 
    // The modern macOS App UI is currently unavaiable in the Version 
    // 0.99.0.0 of Xamarin Workbooks. This feature will be restored in a 
    // future release.
    // _________________________________________________________________
	/// <summary>
	/// Gets the new window for tab.
	/// </summary>
	/// <param name="sender">Sender.</param>
	// public override void GetNewWindowForTab (NSObject sender)
	// {
	// 	// Create a new window when the Plus button is clicked
	// 	ToolbarResources.OpenNewTraditionalWindow ();
	// }
	#endregion
}

/// <summary>
/// Modern code based window that uses the modern look and feel
/// of a macOS Sierra Window. Typically this Window would be created in a Xcode
/// Interface Builder `.storyboard` file. For the sake of this workbook,
/// the Window is being created in code.
/// </summary>
public class ModernCodeBasedWindow : NSWindow
{

	#region Computed Properties
	/// <summary>
	/// Gets or sets the click me label.
	/// </summary>
	/// <value>The click me label.</value>
	public NSTextField ClickMeLabel { get; set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="handle">Handle.</param>
	public ModernCodeBasedWindow (IntPtr handle) : base (handle)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="coder">Coder.</param>
	[Export ("initWithCoder:")]
	public ModernCodeBasedWindow (NSCoder coder) : base (coder)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindow"/> class.
	/// </summary>
	/// <param name="contentRect">Content rect.</param>
	/// <param name="aStyle">A style.</param>
	/// <param name="bufferingType">Buffering type.</param>
	/// <param name="deferCreation">If set to <c>true</c> defer creation.</param>
	public ModernCodeBasedWindow (CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base (contentRect, aStyle, bufferingType, deferCreation)
	{
		// Create a default title for the window
		Title = "Untitled";

		// Select the dark appearance
		Appearance = NSAppearance.GetAppearance (NSAppearance.NameVibrantDark);

		// Create the content view for the window and make it fill the window
		ContentView = new LayerBackedView (Frame);

		// Hide the Title bar for a streamlined UI
		TitleVisibility = NSWindowTitleVisibility.Hidden;

		// _________________________________________________________________
        // WARNING! 
        // The modern macOS App UI is currently unavaiable in the Version 
        // 0.99.0.0 of Xamarin Workbooks. This feature will be restored in a 
        // future release.
        // _________________________________________________________________
		// Configure the Window to use Tabs
		// TabbingMode = NSWindowTabbingMode.Preferred;
		// TabbingIdentifier = "Modern";

		// Define Window UI
		ClickMeLabel = new NSTextField (new CGRect (10, Frame.Height - 125, Frame.Width - 20, 20)) {
			BackgroundColor = NSColor.Clear,
			TextColor = NSColor.LabelColor,
			Editable = false,
			Bezeled = false,
			AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.MinYMargin,
			StringValue = "Click on a Toolbar item above."
		};
		ContentView.AddSubview (ClickMeLabel);
	}
	#endregion

	#region Override Method
	/// <summary>
	/// Awakes from nib.
	/// </summary>
	public override void AwakeFromNib ()
	{
		base.AwakeFromNib ();

		// Build and configure a new toolbar
		var toolbar = new MainToolbar ("ModernToolbar");

		// Wireup toolbar events
		toolbar.ToolbarDelegate.ToolbarItemClicked += (identifier) => {
			ClickMeLabel.StringValue = $"You clicked the {identifier} Toolbar Item.";
		};

		// Attach toolbar to window
		this.Toolbar = toolbar;
	}
	#endregion
}

/// <summary>
/// Modern code based Window Controller instantiats and controls an instance of
/// the `ModernCodeBasedWindow` defined above.
/// </summary>
public class ModernCodeBasedWindowController : NSWindowController
{
	#region Computed Properties
	/// <summary>
	/// Gets the window.
	/// </summary>
	/// <value>The window.</value>
	public new ModernCodeBasedWindow Window {
		get { return base.Window as ModernCodeBasedWindow; }
	}
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindowController"/> class.
	/// </summary>
	/// <param name="handle">Handle.</param>
	public ModernCodeBasedWindowController (IntPtr handle) : base (handle)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindowController"/> class.
	/// </summary>
	/// <param name="coder">Coder.</param>
	[Export ("initWithCoder:")]
	public ModernCodeBasedWindowController (NSCoder coder) : base (coder)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ModernCodeBasedWindowController"/> class.
	/// </summary>
	public ModernCodeBasedWindowController () : base ("CodeBasedWindow")
	{
		// Define the Window's default location and size
		CGRect contentRect = new CGRect (0, 100, 600, 500);

		// Create a new instance of the CodeBasedWindow
		base.Window = new ModernCodeBasedWindow (contentRect, (NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.FullSizeContentView | NSWindowStyle.UnifiedTitleAndToolbar), NSBackingStore.Buffered, false);

		// Simulate Awaking from Nib
		Window.AwakeFromNib ();
	}
	#endregion

	#region Override Methods
	/// <summary>
	/// Awakes from nib.
	/// </summary>
	public override void AwakeFromNib ()
	{
		base.AwakeFromNib ();
	}

	// _________________________________________________________________
    // WARNING! 
    // The modern macOS App UI is currently unavaiable in the Version 
    // 0.99.0.0 of Xamarin Workbooks. This feature will be restored in a 
    // future release.
    // _________________________________________________________________
	/// <summary>
	/// Gets the new window for tab.
	/// </summary>
	/// <param name="sender">Sender.</param>
	// public override void GetNewWindowForTab (NSObject sender)
	// {
	// 	// Create a new window when the Plus button is clicked
	// 	ToolbarResources.OpenNewModernWindow ();
	// }
	#endregion
}

/// <summary>
/// Toolbar resources is a static helper class that creates and maintains
/// instances of the Windows, View Controllers and Views needed to support
/// playing with a `NSToolbar` control in a Workbook.
/// </summary>
public static class ToolbarResources
{
	#region Computed Properties
	/// <summary>
	/// Gets or sets the traditional window count.
	/// </summary>
	/// <value>The traditional window count.</value>
	public static int TraditionalWindowCount { get; set; } = 0;

	/// <summary>
	/// Gets or sets the last traditional window controller.
	/// </summary>
	/// <value>The last traditional window controller.</value>
	public static TraditionalCodeBasedWindowController LastTraditionalWindowController { get; set; } = null;

	/// <summary>
	/// Gets or sets the modern window count.
	/// </summary>
	/// <value>The modern window count.</value>
	public static int ModernWindowCount { get; set; } = 0;

	/// <summary>
	/// Gets or sets the last modern window controller.
	/// </summary>
	/// <value>The last modern window controller.</value>
	public static ModernCodeBasedWindowController LastModernWindowController { get; set; } = null;
	#endregion

	#region Controller Factories
	/// <summary>
	/// Opens and displays a new traditional window.
	/// </summary>
	public static void OpenNewTraditionalWindow ()
	{
		// Create a new instace of the Traditional Window Controller
		LastTraditionalWindowController = new TraditionalCodeBasedWindowController ();

		// Set the title
		LastTraditionalWindowController.Window.Title = (TraditionalWindowCount++ == 0) ? "Untitled" : $"Untitled {TraditionalWindowCount}";
	
		// Display the Window
		LastTraditionalWindowController.Window.MakeKeyAndOrderFront ((NSObject)NSApplication.SharedApplication.Delegate);
	}

	/// <summary>
	/// Opens and displays a new modern window.
	/// </summary>
	public static void OpenNewModernWindow ()
	{
		// Create a new instace of the Traditional Window Controller
		LastModernWindowController = new ModernCodeBasedWindowController ();

		// Set the title
		LastModernWindowController.Window.Title = (ModernWindowCount++ == 0) ? "Untitled" : $"Untitled {ModernWindowCount}";

		// Display the Window
		LastModernWindowController.Window.MakeKeyAndOrderFront ((NSObject)NSApplication.SharedApplication.Delegate);
	}
	#endregion
}
