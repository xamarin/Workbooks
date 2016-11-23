using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using Foundation;
using AppKit;
using ObjCRuntime;

// -------------------------------------------------------------------------
// StoryboardInflator for macOS by Kevin Mullins for Microsoft, Inc.
// -------------------------------------------------------------------------
// This library provides limited support for using Storyboard files to
// build and display the user interface for a Xamarin.Mac based workbook app. 
// Because macOS expects a compiled storyboard to be part of a running app's 
// bundle along with its executable code, the typical mechanisms to create
// Outlets, Actions and Segues will not work. Instead, this library applies
// the following hack to simulate Outlets:
//
// 1) Windows and Views (or View based controls such as NSButton), set their
//    Identifier property in Interface Builder to match the name of the 
//    "Outlet". The StoryboardBinder will populate any like named Public
//    Property on any class it binds.
//
// 2) For Menu Items, the StoryboardBinder will use the Title of the Menu
//    Item + "MenuItem" to populate any like named Public Property on any
//    class that it binds. For example, the "New" menu item would bind to
//    the "NewMenuItem" property in class.
//
// 3) For Toolbar Items, the StoryboardBinder will use the Label of the 
//    Item + "ToolbarItem" to populate any like named Public Property on any
//    class that it binds. For example, the "Color" menu item would bind to
//    the "ColorMenuItem" property in class.
//
// LIMITATIONS: Custom Segues are currently not supported. Instead, the app must
// use the StoryboardInflator to inflate and populate any NIB that will then
// need to be manually displayed.

/// <summary>
/// Defines the type of a Scene or Source Object used in a Segue. 
/// </summary>
public enum StoryboardObjectType
{
	/// <summary>
	/// The application (<c>NSApplication</c>) as defined in the Storyboard.
	/// </summary>
	Application,

	/// <summary>
	/// A Menu Item (<c>NSMenuItem</c>).
	/// </summary>
	MenuItem,

	/// <summary>
	/// A Window Controller (<c>NSWindowController</c>).
	/// </summary>
	WindowController,

	/// <summary>
	/// A Toolbar Item (<c>NSToolbarItem</c>).
	/// </summary>
	ToolbarItem,

	/// <summary>
	/// A View Controller (<c>NSViewController</c>).
	/// </summary>
	ViewController,

	/// <summary>
	/// Storyboard object type (<c>NSSplitViewController</c>).
	/// </summary>
	SplitViewController,

	/// <summary>
	/// A Button (<c>NSButton</c>).
	/// </summary>
	Button,

	/// <summary>
	/// An unknown object type
	/// </summary>
	Unknown
}

/// <summary>
/// Defines the kind of Segue as defined in the Storyboard.
/// </summary>
public enum StoryboardSegueType
{
	/// <summary>
	/// Displays the destination controller as a non-modal Window.
	/// </summary>
	Show,

	/// <summary>
	/// Displays the destination controller as a modal window.
	/// </summary>
	Modal,

	/// <summary>
	/// Displays the destination controller as a sheet.
	/// </summary>
	Sheet,

	/// <summary>
	/// Displays the destination controller as a popover.
	/// </summary>
	Popover,

	/// <summary>
	/// Displays the destination controller using a Custom Segue Class.
	/// </summary>
	Custom,

	/// <summary>
	/// Defines a containment relationship between the source object and the
	/// destination controller.
	/// </summary>
	Relationship
}

/// <summary>
/// Holds the definition of a Segure as read from a Storyboard.
/// </summary>
public class StoryboardSegueDefinition : NSObject
{
	#region Computed Properties
	/// <summary>
	/// Gets or sets the inflator used to load NIBs from the Storyboard.
	/// </summary>
	/// <value>The inflator.</value>
	public StoryboardInflator Inflator { get; set; }

	/// <summary>
	/// Gets or sets the type of the scene that they segue belongs to.
	/// </summary>
	/// <value>The type of the scene.</value>
	public StoryboardObjectType SceneType { get; set;}

	/// <summary>
	/// Gets or sets the scene identifier.
	/// </summary>
	/// <value>The scene identifier.</value>
	public string SceneID { get; set; }

	/// <summary>
	/// Gets or sets the type of the source object.
	/// </summary>
	/// <value>The type of the source object.</value>
	public StoryboardObjectType SourceObjectType { get; set;}

	/// <summary>
	/// Gets or sets the source object identifier.
	/// </summary>
	/// <value>The source object identifier.</value>
	public string SourceObjectID { get; set; }

	/// <summary>
	/// Gets or sets the destination controller identifier.
	/// </summary>
	/// <value>The destination controller identifier.</value>
	public string DestinationControllerID { get; set;}

	/// <summary>
	/// Gets or sets the kind of the segue.
	/// </summary>
	/// <value>The kind of the segue.</value>
	public StoryboardSegueType SegueKind { get; set; }

	/// <summary>
	/// Gets or sets the segue identifier.
	/// </summary>
	/// <value>The segue identifier.</value>
	public string SegueIdentifier { get; set;}

	/// <summary>
	/// Gets or sets the relationship.
	/// </summary>
	/// <remarks>This is only populated from containment Segues of <c>StoryboardSegueType.Relationship</c>.</remarks>
	/// <value>The relationship.</value>
	public string Relationship { get; set;}

	/// <summary>
	/// Gets or sets the popover anchor identifier.
	/// </summary>
	/// <value>The popover anchor identifier.</value>
	public string PopoverAnchorID { get; set;}

	/// <summary>
	/// Gets or sets the popover behavior.
	/// </summary>
	/// <value>The popover behavior.</value>
	public NSPopoverBehavior PopoverBehavior { get; set; } = NSPopoverBehavior.Transient;

	/// <summary>
	/// Gets or sets the edge the popover will be displayed from.
	/// </summary>
	/// <remarks>0 = Left, 1 = Top, 2 = Right, 3 = Bottom</remarks>
	/// <value>The popover edge.</value>
	public nuint PopoverEdge { get; set; } = 0;
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:StoryboardSegueDefinition"/> class.
	/// </summary>
	/// <param name="type">The <c>StoryboardSegueType</c> of the Segue.</param>
	/// <param name="inflator">The Inflator used to load NIB files.</param>
	public StoryboardSegueDefinition (StoryboardSegueType type, StoryboardInflator inflator)
	{
		// Initialize
		SegueKind = type;
		Inflator = inflator;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:StoryboardSegueDefinition"/> class.
	/// </summary>
	/// <param name="type">The <c>string</c> type of the Segue.</param>
	/// <param name="inflator">The Inflator used to load NIB files.</param>
	public StoryboardSegueDefinition (string type, StoryboardInflator inflator)
	{
		// Initialize
		switch (type) {
		case "show":
			SegueKind = StoryboardSegueType.Show;
			break;
		case "modal":
			SegueKind = StoryboardSegueType.Modal;
			break;
		case "sheet":
			SegueKind = StoryboardSegueType.Sheet;
			break;
		case "popover":
			SegueKind = StoryboardSegueType.Popover;
			break;
		case "custom":
			SegueKind = StoryboardSegueType.Relationship;
			break;
		}
		Inflator = inflator;
	}
	#endregion

	#region Private Methods
	/// <summary>
	/// Prepares to execute a segue loading the destination NIB and calling <c>PrepareForSegue</c>
	/// on the Source Controller so it can prepare the destination controller before it is
	/// presented to the user.
	/// </summary>
	/// <returns>The <c>NSStoryboardSegue</c> representing this Segue Definition.</returns>
	/// <param name="sender">The object that is launching the segue.</param>
	/// <param name="sourceController">Source controller for the segue.</param>
	private NSStoryboardSegue PrepareForSegue (NSObject sender, NSObject sourceController)
	{
		// Attempt to load destination
		var destinationController = Inflator.InstantiateControllerForPartialIdentifier (DestinationControllerID);

		// Was the NIB found?
		if (destinationController == null) return null;

		// Build new Segue
		var segue = new NSStoryboardSegue (SegueIdentifier, sourceController, destinationController);

		// Does the class contain the PrepareForSegue method?
		var controllerType = sourceController.GetType ();
		var methodInfo = controllerType.GetMethod ("PrepareForSegue");
		if (methodInfo != null) {
			// Yes, wireup action to class
			methodInfo.Invoke (sourceController, new [] { segue, sender });
		}

		// Return controller
		return segue;
	}

	/// <summary>
	/// Presents the non modal window to the user.
	/// </summary>
	/// <param name="segue">The <c>NSStoryboardSegue</c> to execute.</param>
	private void PresentNonModalWindow (NSStoryboardSegue segue)
	{
		NSWindowController windowController = null;

		// Take action based on the controller type
		if (segue.DestinationController is NSWindowController) {
			// Display the window to the user
			windowController = segue.DestinationController as NSWindowController;
		} else if (segue.DestinationController is NSViewController) {
			// Build a Window and Window Controller for this view
			var viewController = segue.DestinationController as NSViewController;
			var window = new NSWindow (viewController.View.Bounds, (NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable), NSBackingStore.Buffered, false);
			windowController = new NSWindowController (window);

			// Attach the View Controller to the Window
			window.ContentView = viewController.View;
			window.ContentViewController = viewController;
		}

		// Found?
		if (windowController == null) return;

		// Present window controller
		windowController.Window.MakeKeyAndOrderFront ((NSObject)NSApplication.SharedApplication.Delegate);
	}

	/// <summary>
	/// Presents the modal window to the user.
	/// </summary>
	/// <param name="segue">The <c>NSStoryboardSegue</c> to execute.</param>
	private void PresentModalWindow (NSStoryboardSegue segue)
	{
		NSWindowController windowController = null;

		// Take action based on the controller type
		if (segue.DestinationController is NSWindowController) {
			// Display the window to the user
			windowController = segue.DestinationController as NSWindowController;
		} else if (segue.DestinationController is NSViewController) {
			// Build a Window and Window Controller for this view
			var viewController = segue.DestinationController as NSViewController;
			var window = new NSWindow (viewController.View.Bounds, (NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable), NSBackingStore.Buffered, false);
			windowController = new NSWindowController (window);

			// Attach the View Controller to the Window
			window.ContentView = viewController.View;
			window.ContentViewController = viewController;
		}

		// Found?
		if (windowController == null) return;

		// Present window controller
		NSApplication.SharedApplication.RunModalForWindow (windowController.Window);
	}

	/// <summary>
	/// Presents the Window or View to the user as a sheet attached to the parent
	/// Window.
	/// </summary>
	/// <param name="segue">The <c>NSStoryboardSegue</c> to execute.</param>
	private void PresentSheet (NSStoryboardSegue segue)
	{
		NSViewController viewController = null;

		// Take action based on the controller type
		if (segue.DestinationController is NSWindowController) {
			// Display the window to the user
			var windowController = segue.DestinationController as NSWindowController;
			viewController = windowController.Window.ContentViewController;
		} else if (segue.DestinationController is NSViewController) {
			// Grab view controller
			viewController = segue.DestinationController as NSViewController;
		}

		// Found?
		if (viewController == null) return;

		// Present window controller
		if (segue.SourceController is NSWindowController) {
			var sourceController = segue.SourceController as NSWindowController;
			sourceController.Window.ContentViewController.PresentViewControllerAsSheet (viewController);
		} else if (segue.SourceController is NSViewController) {
			var sourceController = segue.SourceController as NSViewController;
			sourceController.PresentViewControllerAsSheet (viewController);
		}
	}

	/// <summary>
	/// Presents the Window or View to the user as a popover attached to a parent
	/// View.
	/// </summary>
	/// <param name="segue">The <c>NSStoryboardSegue</c> to execute.</param>
	/// <param name="sender">The <c>NSView</c> based element that the popover will be attached to.</param>
	private void PresentPopover (NSStoryboardSegue segue, NSObject sender)
	{
		NSViewController viewController = null;
		NSView view = null;

		// Take action based on the controller type
		if (segue.DestinationController is NSWindowController) {
			// Display the window to the user
			var windowController = segue.DestinationController as NSWindowController;
			viewController = windowController.Window.ContentViewController;
		} else if (segue.DestinationController is NSViewController) {
			// Grab view controller
			viewController = segue.DestinationController as NSViewController;
		}

		// Found?
		if (viewController == null) return;

		// Take action based on sender type
		if (sender is NSToolbarItem) {
			var item = sender as NSToolbarItem;
			if (item.View == null) {
				// Default to presenting as a sheet
				PresentSheet (segue);
				return;
			} else {
				// Use the items view
				view = item.View;
			}
		} else {
			// It's a view based control
			view = sender as NSView;
		}

		// Present window controller
		if (segue.SourceController is NSWindowController) {
			var sourceController = segue.SourceController as NSWindowController;
			sourceController.Window.ContentViewController.PresentViewController (viewController, viewController.View.Bounds, view, PopoverEdge, PopoverBehavior);
		} else if (segue.SourceController is NSViewController) {
			var sourceController = segue.SourceController as NSViewController;
			sourceController.PresentViewController (viewController, viewController.View.Bounds, view, PopoverEdge, PopoverBehavior);
		}
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Performs the segue as specified in this Segue Definition.
	/// </summary>
	/// <param name="sender">The object that is launching the segue.</param>
	/// <param name="sourceController">Source controller for the segue.</param>
	public void PerformSegue (NSObject sender, NSObject sourceController)
	{

		//NSApplication.SharedApplication.KeyWindow.Title = "Loading Segue"; 

		// Prepare for segue
		var segue = PrepareForSegue (sender, sourceController);

		// Did the NIB load?
		if (segue == null) return;

		//NSApplication.SharedApplication.KeyWindow.Title = "Segue Loaded";

		// Take action based on the Segue type
		switch (SegueKind) {
		case StoryboardSegueType.Show:
			PresentNonModalWindow (segue);
			break;
		case StoryboardSegueType.Modal:
			PresentModalWindow (segue);
			break;
		case StoryboardSegueType.Sheet:
			PresentSheet (segue);
			break;
		case StoryboardSegueType.Popover:
			PresentPopover (segue, sender);
			break;
		}
	}

	/// <summary>
	/// Sets the popover behavior.
	/// </summary>
	/// <param name="behavior">Behavior as a string value.</param>
	public void SetPopoverBehavior (string behavior)
	{
		// Anything to do?
		if (behavior == null) return;

		// Take action based on value
		switch (behavior) {
		case "t":
			PopoverBehavior = NSPopoverBehavior.Transient;
			break;
		case "semitransient":
			PopoverBehavior = NSPopoverBehavior.Semitransient;
			break;
		case "applicationDefined":
			PopoverBehavior = NSPopoverBehavior.ApplicationDefined;
			break;
		}
	}

	/// <summary>
	/// Sets the popover edge.
	/// </summary>
	/// <param name="edge">Edge as a string value.</param>
	public void SetPopoverEdge (string edge)
	{
		// Anything to do?
		if (edge == null) return;

		// Take action based on value
		switch (edge) {
		case "minX":
			PopoverEdge = 0;
			break;
		case "minY":
			PopoverEdge = 1;
			break;
		case "maxX":
			PopoverEdge = 2;
			break;
		case "maxY":
			PopoverEdge = 3;
			break;
		}
	}
	#endregion
}

/// <summary>
/// Helper class used to compile a storyboard included in a workbook.
/// </summary>
public static class StoryboardCompiler
{
	#region Public Methods
	/// <summary>
	/// Compiles the specified storyboardName.
	/// </summary>
	/// <param name="storyboardName">The Storyboard name of the storyboard to compile without the `.storyboard` extension.</param>
	public static void Compile (string storyboardName)
	{
		// Assemble the Interface Builder compiler call
		var dir = Directory.GetCurrentDirectory ();
		var command = "ibtool";
		var arguments = $"\"{dir}/{storyboardName}.storyboard\" --compile \"{dir}/{storyboardName}.storyboardc\"";

		// Prepare to call the system to invoke the ibtool
		var startInfo = new ProcessStartInfo () {
			FileName = command,
			Arguments = arguments,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			UserName = System.Environment.UserName
		};

		// Invoke the ibtool and write the results to the
		// console
		using (Process process = Process.Start (startInfo)) { // Monitor for exit}
			process.WaitForExit ();
			using (var output = process.StandardOutput) {
				var results = output.ReadToEnd ();
				Console.WriteLine ("Results: {0}", (results == "") ? "Successful" : results);
			}
		}
	}
	#endregion
}

/// <summary>
/// The StoryboardBinder class provides a mechanism to bind objects inflated from a
/// compiled Storyboard NIB file to classes in a workbook. Currently it can bind:
/// NSMenu, NSToolBar, NSWindowController, NSWindow, NSViewController, NSView and
/// any NSView base controls (such as NSButton).
/// 
/// It uses the following hack to support Outlets: a) NSWindow and NSView based
/// controls are bound to Public Properties matching their Identifier property,
/// b) NSMenuItems are bound to Public Properties matching Title + "MenuItem",
/// c) NSToolbarItems are bound to Public Properties matching Label + "ToolbarItem".
/// 
/// Custom Segues are currently not supported. For many standard items (such as
/// NSButton and NSImageView), images will attempted to be bound to `.png` files of
/// the same name in an `Images` directory in the workbooks directory.
/// </summary>
public static class StoryboardBinder 
{
	#region Public Methods
	/// <summary>
	/// Bind the specified storyboardClass and workbookClass. For high-level classes
	/// like NSWindowController and NSViewController, the binder will walk down the
	/// tree of objects binding lower-level items along the chain.
	/// </summary>
	/// <param name="storyboardClass">The high-level storyboard (or workbook) class to bind from.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void Bind (NSObject storyboardClass, NSObject workbookClass)
	{
		// Take action based on the source object type
		if (storyboardClass is NSMenu) {
			BindMenu (storyboardClass as NSMenu, workbookClass);
		} else if (storyboardClass is NSWindowController) {
			BindWindowController (storyboardClass as NSWindowController, workbookClass);
		} else if (storyboardClass is NSWindow) {
			BindWindow (storyboardClass as NSWindow, workbookClass);
		} else if (storyboardClass is NSViewController) {
			BindViewController (storyboardClass as NSViewController, workbookClass);
		} else if (storyboardClass is NSView) {
			BindView (storyboardClass as NSView, workbookClass);
		} else if (storyboardClass is NSToolbar) {
			BindToolbar (storyboardClass as NSToolbar, workbookClass);
		}
	}

	/// <summary>
	/// Attempts to bind an NSImage that has been specified in Interface Builder in the
	/// form `TheImage.png` to a like-named image file in the `Images` directory in the
	/// same directory as the workbook.
	/// </summary>
	/// <returns>The NSImage loaded from the `Images` directory or the passed-in NSImage if not found.</returns>
	/// <param name="image">The source NSImage as specified in Interface Builder.</param>
	public static NSImage BindImage (NSImage image)
	{
		// Has an image been specified?
		if (image !=null && image.Name != null && image.Name.Contains (".png")) {
			// Yes, attempt load the image resource
			var imageName = $"Images/{image.Name}";

			// Does the file exist?
			if (File.Exists (imageName)) {
				// Yes, load new image
				image = new NSImage (imageName);
			}
		}

		return image;
	}

	/// <summary>
	/// Binds any Menu Item to a Public Property in a workbook class that matches
	/// Title + "MenuItem". For example, the "New" Menu Item to the "NewMenuItem"
	/// Property.
	/// </summary>
	/// <remarks>Any image specified in an item will also attemp to be bound.</remarks>
	/// <param name="menu">The NSMenu to bind.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void BindMenu (NSMenu menu, NSObject workbookClass)
	{
		// Bind all menu items
		for (nint n = 0; n < menu.Count; ++n) {
			var menuItem = menu.ItemAt (n);

			// Bind identity using a hack
			var propertyName = $"{menuItem.Title.Replace (" ", "").Replace ("…", "")}MenuItem";
			BindProperty (propertyName, workbookClass, menuItem);

			// Bind image
			menuItem.Image = BindImage (menuItem.Image);

			// Bind actions
			BindAction (menuItem.Action, workbookClass, menuItem);

			// Has segue?
			if (menuItem.Target is StoryboardSegueDefinition) {
				var segue = menuItem.Target as StoryboardSegueDefinition;

				// Bind it
				menuItem.Activated += (sender, e) => {
					segue.PerformSegue (menuItem, workbookClass);
				};
			}

			// Scan sub menus
			if (menuItem.Submenu != null) {
				BindMenu (menuItem.Submenu, workbookClass);
			}
		}
		
	}

	/// <summary>
	/// Binds any Toolbar Item to A Public Property in a workbook class that matches
	/// Label + "ToolbarItem". For example, the "Color" Toolbar Item to the "ColorToolbarItem"
	/// Property.
	/// </summary>
	/// <remarks>Any image specified in an item will also attemp to be bound.</remarks>
	/// <param name="toolbar">The NSToolbar to bind.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void BindToolbar (NSToolbar toolbar, NSObject workbookClass)
	{
		// Bind all items on the Toolbar
		foreach (NSToolbarItem item in toolbar.Items) {
			// Bind identity using a hack
			var propertyName = $"{item.Label.Replace (" ", "")}ToolbarItem";
			BindProperty (propertyName, workbookClass, item);

			// Bind Images
			item.Image = BindImage (item.Image);

			// Bind actions
			BindAction (item.Action, workbookClass, item);

			// Has segue?
			if (item.Target is StoryboardSegueDefinition) {
				var segue = item.Target as StoryboardSegueDefinition;

				// Bind it
				item.Activated += (sender, e) => {
					segue.PerformSegue (item, workbookClass);
				};
			}
		}
	}

	/// <summary>
	/// Binds the Window Controller to the given workbook class. This method will walk down the
	/// tree of objects binding lower-level classes as well.
	/// </summary>
	/// <param name="controller">The NSWindowController to bind from.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void BindWindowController (NSWindowController controller, NSObject workbookClass)
	{
		// Bind controlled window
		BindWindow (controller.Window, workbookClass);

		// Binding to a Window Controller?
		if (workbookClass is NSWindowController) {
			// Yes, bind controllers
			var windowController = workbookClass as NSWindowController;
			windowController.Window = controller.Window;

			// Is the window loaded?
			if (windowController.IsWindowLoaded) {
				// Simulate the window being loaded by the workbook's controller instance
				windowController.WindowWillLoad ();
				windowController.WindowDidLoad ();
			}
		}

	}

	/// <summary>
		/// Binds the Window to the given workbook class. This method will walk down the tree
		/// of objects binding lower-level classes as well.
		/// </summary>
		/// <param name="window">The NSWindow to bind from.</param>
		/// <param name="workbookClass">The workbook class to bind to.</param>
		public static void BindWindow (NSWindow window, NSObject workbookClass)
		{
			// Bind identity
			BindProperty (window.Identifier, workbookClass, window);

			// Does the window have a toolbar?
			if (window.Toolbar != null) {
				// Yes, bind it
				BindToolbar (window.Toolbar, workbookClass);
			}

			// Binding to a window instance?
			if (workbookClass is NSWindow && workbookClass.GetType ().Name == window.Identifier) {
				// Yes, bind objects
				var wbWindow = workbookClass as NSWindow;
				wbWindow.ContentViewController = window.ContentViewController;
				wbWindow.ContentView = window.ContentView;
			}

			// Does the window contain a Split View
			if (window.ContentView is NSSplitView) {
				// Bind split view contents
				BindSplitView (window.ContentView as NSSplitView, workbookClass);
			} else {
				// Bind content view controller
				BindViewController (window.ContentViewController, workbookClass);
			}
		}

		/// <summary>
		/// Binds the split view to the given workbook class. This method will walk down the
		/// tree of objects binding lower-level classes as well.
		/// </summary>
		/// <param name="splitView">Split view being bound.</param>
		/// <param name="workbookClass">The workbook class to bind to.</param>
		public static void BindSplitView (NSSplitView splitView, NSObject workbookClass)
		{
			// Bind identity
			BindProperty (splitView.Identifier, workbookClass, splitView);

			// Bind all the attached views
			foreach (NSView view in splitView.ArrangedSubviews) {
				// Bind controlled view
				BindView (view, workbookClass);
			}

			// Binding to a view controller?
			if (workbookClass is NSViewController) {
				// Yes, bind controllers
				var viewController = workbookClass as NSViewController;
				viewController.View = splitView;

				// Simulate the view being loaded by the workbook's controller instance
				viewController.ViewDidLoad ();
				viewController.ViewWillAppear ();
				viewController.ViewDidAppear ();
				viewController.ViewWillLayout ();
				viewController.ViewDidLayout ();
			}
		}

	/// <summary>
	/// Binds the View Controller to the given workbook class. This method will walk down the
	/// tree of objects binding lower-level classes as well.
	/// </summary>
	/// <param name="controller">The NSViewController to bind from.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void BindViewController (NSViewController controller, NSObject workbookClass)
	{
		// Bind controlled view
		BindView (controller.View, workbookClass);

		// Binding to a view controller?
		if (workbookClass is NSViewController) {
			// Yes, bind controllers
			var viewController = workbookClass as NSViewController;
			viewController.View = controller.View;

			// Is the view loaded?
			if (controller.ViewLoaded) {
				// Simulate the view being loaded by the workbook's controller instance
				viewController.ViewDidLoad ();
				viewController.ViewWillAppear ();
				viewController.ViewDidAppear ();
				viewController.ViewWillLayout ();
				viewController.ViewDidLayout ();
			}
		}
	}

	/// <summary>
	/// Binds the View to the given workbook class. This method will transverse all SubViews
	/// in the given tree.
	/// </summary>
	/// <remarks>Any image specified in a known item will also attemp to be bound.</remarks>
	/// <param name="view">The NSView to bind from.</param>
	/// <param name="workbookClass">The workbook class to bind to.</param>
	public static void BindView (NSView view, NSObject workbookClass)
	{
		// Bind identity
		BindProperty (view.Identifier, workbookClass, view);

		// Bind images for known items
		if (view is NSButton) {
			var button = view as NSButton;
			button.Image = BindImage (button.Image);
			BindAction (button.Action, workbookClass, button);

			// Has segue?
			if (button.Target is StoryboardSegueDefinition) {
				var segue = button.Target as StoryboardSegueDefinition;

				// Bind it
				button.Activated += (sender, e) => {
					segue.PerformSegue (button, workbookClass);
				};
			}
		} else if (view is NSPopUpButton) {
			var popup = view as NSPopUpButton;
			foreach (NSMenuItem item in popup.Items ()) {
				item.Image = BindImage (item.Image);
			}
			BindAction (popup.Action, workbookClass, popup);
		} else if (view is NSSegmentedControl) {
			var segment = view as NSSegmentedControl;
			for (nint n = 0; n < segment.SegmentCount; ++n) {
				var image = segment.GetImage (n);
				segment.SetImage (BindImage (image), n);
			}
			BindAction (segment.Action, workbookClass, segment);
		} else if (view is NSImageView) {
			var image = view as NSImageView;
			image.Image = BindImage (image.Image);
		}

		// Bind every subview
		foreach (NSView subview in view.Subviews) {
			BindView (subview, workbookClass);
		}

	}
	#endregion

	#region Private Methods
	/// <summary>
	/// Attempts to bind an object inflated from a compiled Storyboard to an "Outlet"
	/// property on the given workbook class. This hack is a workaround since true
	/// Storyboard Outlets are not supported in workbooks.
	/// </summary>
	/// <param name="propertyName">The name of the Public Property to bind the Outlet to.</param>
	/// <param name="workbookClass">The workbook class being bound to.</param>
	/// <param name="property">The object inflated from the compiled Storyboard.</param>
	private static void BindProperty (string propertyName, NSObject workbookClass, NSObject property)
	{
		// Anything to process?
		if (propertyName == null) return;

		// Does the controller contain the property?
		var controllerType = workbookClass.GetType ();
		var propertyInfo = controllerType.GetProperty (propertyName);
		if (propertyInfo != null && propertyInfo.CanWrite) {
			// Yes, save value in class
			var value = Convert.ChangeType (property, propertyInfo.PropertyType);
			propertyInfo.SetValue (workbookClass, value);
		}
	}

	/// <summary>
	/// Attempts to bind an object inflated from a compiled Storyboard to an "Action"
	/// property on the given workbook class. This hack is a workaround since true
	/// Storyboard Actions are not supported in workbooks.
	/// </summary>
	/// <param name="action">The selector that represents the Action to bind.</param>
	/// <param name="workbookClass">The workbook class being bound to.</param>
	/// <param name="menuItem">The Menu Item that is the target of the binding.</param>
	private static void BindAction (Selector action, NSObject workbookClass, NSMenuItem menuItem)
	{
		// Anything to process?
		if (action == null) return;

		// Switch to .NET style method name
		var actionName = action.Name.Substring(0,1).ToUpper() + action.Name.Substring(1).Replace (":", "");

		// Does the class contain the method?
		var controllerType = workbookClass.GetType ();
		var methodInfo = controllerType.GetMethod (actionName);
		if (methodInfo != null) {
			// Yes, wireup action to class
			menuItem.Activated += (sender, e) => {
				methodInfo.Invoke (workbookClass, new [] { sender });
			};
		}
	}

	/// <summary>
	/// Attempts to bind an object inflated from a compiled Storyboard to an "Action"
	/// property on the given workbook class. This hack is a workaround since true
	/// Storyboard Actions are not supported in workbooks.
	/// </summary>
	/// <param name="action">The selector that represents the Action to bind.</param>
	/// <param name="workbookClass">The workbook class being bound to.</param>
	/// <param name="toolbarItem">The Toolbar Item that is the target of the binding.</param>
	private static void BindAction (Selector action, NSObject workbookClass, NSToolbarItem toolbarItem)
	{
		// Anything to process?
		if (action == null) return;

		// Switch to .NET style method name
		var actionName = action.Name.Substring (0, 1).ToUpper () + action.Name.Substring (1).Replace (":", "");

		// Does the class contain the method?
		var controllerType = workbookClass.GetType ();
		var methodInfo = controllerType.GetMethod (actionName);
		if (methodInfo != null) {
			// Yes, wireup action to class
			toolbarItem.Activated += (sender, e) => {
				methodInfo.Invoke (workbookClass, new [] { sender });
			};
		}
	}

	/// <summary>
	/// Attempts to bind an object inflated from a compiled Storyboard to an "Action"
	/// property on the given workbook class. This hack is a workaround since true
	/// Storyboard Actions are not supported in workbooks.
	/// </summary>
	/// <param name="action">The selector that represents the Action to bind.</param>
	/// <param name="workbookClass">The workbook class being bound to.</param>
	/// <param name="control">The control that is the target of the binding.</param>
	private static void BindAction (Selector action, NSObject workbookClass, NSControl control)
	{
		// Anything to process?
		if (action == null) return;

		// Switch to .NET style method name
		var actionName = action.Name.Substring (0, 1).ToUpper () + action.Name.Substring (1).Replace (":", "");

		// Does the class contain the method?
		var controllerType = workbookClass.GetType ();
		var methodInfo = controllerType.GetMethod (actionName);
		if (methodInfo != null) {
			// Yes, wireup action to class
			control.Activated += (sender, e) => {
				methodInfo.Invoke (workbookClass, new [] { sender });
			};
		}
	}
	#endregion
}

/// <summary>
/// Helper class that can inflate Views and View Controllers from a compiled Storyboard
/// that has been included in a workbook.
/// </summary>
public class StoryboardInflator : NSObject
{
	#region Computed Properies
	/// <summary>
	/// Gets or sets the first responder that will act as the parent to all objects
	/// instantiated from the Storyboard.
	/// </summary>
	/// <value>The first responder.</value>
	public NSObject FirstResponder { get; set; }

	/// <summary>
	/// Gets or sets the bundle that contains the compiled Storyboard.
	/// </summary>
	/// <value>The bundle.</value>
	public NSBundle Bundle { get; set; }

	/// <summary>
	/// Gets or sets the main menu identifier.
	/// </summary>
	/// <value>The main menu identifier.</value>
	public string MainMenuIdentifier { get; set; }

	/// <summary>
	/// Gets or sets the entry point identifier for the initial object specified in
	/// Storyboard.
	/// </summary>
	/// <value>The entry point identifier.</value>
	public string EntryPointIdentifier { get; set; }

	/// <summary>
	/// Gets or sets the view controller identifiers to nib names dictionary.
	/// </summary>
	/// <value>The view controller identifiers to nib names.</value>
	public NSDictionary ViewControllerIdentifiersToNibNames { get; set; }

	/// <summary>
	/// Gets or sets the view controller identifiers to UUID dictionary.
	/// </summary>
	/// <value>The view controller identifiers to UUID.</value>
	public NSDictionary ViewControllerIdentifiersToUUIDs { get; set; }

	/// <summary>
	/// Gets or sets the segue definitions that were defined in the Storyboard.
	/// </summary>
	/// <value>The segue definition collection.</value>
	public List<StoryboardSegueDefinition> SegueDefinitions { get; set; } = new List<StoryboardSegueDefinition> ();
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="T:StoryboardInflator"/> class.
	/// </summary>
	/// <remarks>This constructor sets the First Responder to the AppDelegate.</remarks>
	/// <param name="bundleFile">The Bundle file that contains the compiled Storyboard.</param>
	public StoryboardInflator (string bundleFile)
	{

		// Initialize
		Initialize (bundleFile, (NSObject)NSApplication.SharedApplication.Delegate);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:StoryboardInflator"/> class.
	/// </summary>
	/// <param name="bundleFile">The Bundle file that contains the compiled Storyboard.</param>
	/// <param name="owner">The object that will act as the owner (First Responder) of
	/// any object instantiated from the Storyboard.</param>
	public StoryboardInflator (string bundleFile, NSObject owner)
	{

		// Initialize
		Initialize (bundleFile, owner);
	}

	/// <summary>
	/// Initialize the specified bundle file and owner.
	/// </summary>
	/// <param name="bundleFile">The Bundle file that contains the compiled Storyboard.</param>
	/// <param name="owner">The object that will act as the owner (First Responder) of
	/// any object instantiated from the Storyboard.</param>
	internal void Initialize (string bundleFile, NSObject owner)
	{

		// Initialize
		LoadSegueDefinitions (bundleFile);
		Bundle = new NSBundle (bundleFile);
		FirstResponder = owner;

		// Read the structure of the Storyboard
		EntryPointIdentifier = Bundle.InfoDictionary.ObjectForKey (new NSString ("NSStoryboardDesignatedEntryPointIdentifier")).ToString ();
		MainMenuIdentifier = Bundle.InfoDictionary.ObjectForKey (new NSString ("NSStoryboardMainMenu")).ToString ();
		ViewControllerIdentifiersToNibNames = Bundle.InfoDictionary.ObjectForKey (new NSString ("NSViewControllerIdentifiersToNibNames")) as NSDictionary;
		ViewControllerIdentifiersToUUIDs = Bundle.InfoDictionary.ObjectForKey (new NSString ("NSViewControllerIdentifiersToNibNames")) as NSDictionary;

	}
	#endregion

	#region Private Methods
	/// <summary>
	/// Loads the segue definitions from the non-compiled version of the Storyboard.
	/// </summary>
	/// <param name="bundleFile">The Bundle file that contains the compiled Storyboard.</param>
	private void LoadSegueDefinitions (string bundleFile)
	{
		// Open the non-compiled Storyboard
		var path = bundleFile.Replace (".storyboardc", ".storyboard");
		var reader = new XmlTextReader (path);

		// Track the tree of objects that the Segue is defined in
		var parsingObject = StoryboardObjectType.Unknown;
		var parsingID = "";
		var sceneType = StoryboardObjectType.Unknown;
		var sceneID = "";

		// Read through the non-compiled Storyboard to find all Segue
		// definitions
		while (reader.Read ()) {
			// Take action based on node type
			switch (reader.Name) {
			case "application":
				sceneType = StoryboardObjectType.Application;
				var appID = reader.GetAttribute ("id");
				if (appID != null) {
					sceneID = appID;
				}
				break;
			case "menuItem":
				var title = reader.GetAttribute ("title");
				if (title != null) {
					parsingObject = StoryboardObjectType.MenuItem;
					parsingID = title;
				}
				break;
			case "windowController":
				sceneType = StoryboardObjectType.WindowController;
				var wcID = reader.GetAttribute ("id");
				if (wcID != null) {
					parsingObject = StoryboardObjectType.WindowController;
					parsingID = wcID;
					sceneID = wcID;
				}
				break;
			case "toolbarItem":
				var tbID = reader.GetAttribute ("label");
				if (tbID != null) {
					parsingObject = StoryboardObjectType.ToolbarItem;
					parsingID = tbID;
				}
				break;
			case "viewController":
				sceneType = StoryboardObjectType.ViewController;
				var vcID = reader.GetAttribute ("id");
				if (vcID != null) {
					sceneID = vcID;
				}
				break;
			case "splitViewController":
				sceneType = StoryboardObjectType.SplitViewController;
				var svcID = reader.GetAttribute ("id");
				if (svcID != null) {
					sceneID = svcID;
				}
				break;
			case "button":
				var buttonID = reader.GetAttribute ("identifier");
				if (buttonID != null) {
					parsingObject = StoryboardObjectType.Button;
					parsingID = buttonID;
				}
				break;
			case "segue":
				// Read the Segue's Definition
				var destination = reader.GetAttribute ("destination");
				var kind = reader.GetAttribute ("kind");
				var identifier = reader.GetAttribute ("identifier");
				var id = reader.GetAttribute ("id");
				var relationship = reader.GetAttribute ("relationship");
				var popoverAnchor = reader.GetAttribute ("popoverAnchorView");
				var popoverBehavior = reader.GetAttribute ("popoverBehavior");
				var popoverEdge = reader.GetAttribute ("preferredEdge");

				// Create a new definition, populate it and add it to
				// the collection
				var definition = new StoryboardSegueDefinition (kind, this) {
					SceneType = sceneType,
					SceneID = sceneID,
					SourceObjectType = parsingObject,
					SourceObjectID = parsingID,
					DestinationControllerID = destination,
					SegueIdentifier = (identifier == null) ? id : identifier,
					Relationship = relationship,
					PopoverAnchorID = popoverAnchor
				};
				definition.SetPopoverBehavior (popoverBehavior);
				definition.SetPopoverEdge (popoverEdge);
				SegueDefinitions.Add (definition);
				break;
			}
		}
	}

	/// <summary>
	/// Pulls the type name from the description.
	/// </summary>
	/// <returns>The class type name.</returns>
	/// <remarks>This method is used for debugging.</remarks>
	/// <param name="description">The description.</param>
	private string TypeName (string description)
	{

		var posn = description.IndexOf (":");
		return description.Substring (1, posn - 1);
	}

	/// <summary>
	/// Pulls the class handle pointer from the description.
	/// </summary>
	/// <returns>The handle pointer.</returns>
	/// <remarks>This method is used for debugging.</remarks>
	/// <param name="description">The description.</param>
	private IntPtr ClassHandlePointer (string description)
	{

		var posn = description.IndexOf (":");
		var value = description.Substring (posn + 1, description.Length - posn - 1);
		return Marshal.StringToHGlobalUni (value);
	}

	/// <summary>
	/// Prints the top objects to the console.
	/// </summary>
	/// <remarks>This method is used for debugging.</remarks>
	/// <param name="topLevelObjects">The collection of top level objects.</param>
	private void PrintTopObjects (NSArray topLevelObjects)
	{

		NSObject element = null;

		// Scan all top level object for the window controller
		for (nuint n = 0; n < topLevelObjects.Count; ++n) {
			// Access the current object
			element = topLevelObjects.GetItem<NSObject> (n);
			Console.WriteLine (">   Type: {0}", TypeName (element.Description));
		}
	}

	/// <summary>
	/// Checks every Menu and Menu Item to see if a Segue has been attached to it.
	/// If so, it sets the <c>StoryboardSegueDefinition</c> as the Menu Item's 
	/// <c>Target</c>.
	/// </summary>
	/// <param name="menu">The <c>NSMenu</c> to scan.</param>
	private void LoadMenuSegueTargets (NSMenu menu)
	{
		// Check all items to see if they have been bound to a segue
		for (nint x = 0; x < menu.Count; ++x) {
			var menuItem = menu.ItemAt (x);

			// Has a segue been defined for this item?
			var segueDefinition = FindSegueDefinition (StoryboardObjectType.MenuItem, menuItem.Title);
			if (segueDefinition != null) menuItem.Target = segueDefinition;

			// Scan sub menus
			if (menuItem.Submenu != null) {
				LoadMenuSegueTargets (menuItem.Submenu);
			}
		}
	}

	/// <summary>
	/// Checks every Toolbar Item to see if a Segue has been attached to it.
	/// If so, it sets the <c>StoryboardSegueDefinition</c> as the Toolbar Item's 
	/// <c>Target</c>.
	/// </summary>
	/// <param name="toolbar">The <c>NSToolbar</c> to scan.</param>
	private void LoadToolbarItemSegueTargets (NSToolbar toolbar)
	{
		// Check all items to see if they have been bound to a segue
		foreach (NSToolbarItem item in toolbar.Items) {
			// Has a segue been defined for this item?
			var segueDefinition = FindSegueDefinition (StoryboardObjectType.ToolbarItem, item.Label);
			if (segueDefinition != null) {
				item.Target = segueDefinition;
			}
		}
	}

	/// <summary>
	/// Checks every View and Sub View to see if a Segue has been attached to it.
	/// If so, it sets the <c>StoryboardSegueDefinition</c> as the View's 
	/// <c>Target</c>.
	/// </summary>
	/// <param name="view">The <c>NSView</c> to scan.</param>
	public void LoadViewSegueTargets (NSView view)
	{
		// Bind segues for known items
		if (view is NSButton) {
			var button = view as NSButton;

			// Has a segue been defined for this item?
			var segueDefinition = FindSegueDefinition (StoryboardObjectType.Button, button.Identifier);
			if (segueDefinition != null) {
				button.Target = segueDefinition;
			}
		} 

		// Bind every subview
		foreach (NSView subview in view.Subviews) {
			LoadViewSegueTargets (subview);
		}

	}

	/// <summary>
	/// Loads the NIB from the specified NIB name and recursivly loads any sub NIBs
	/// for a known set of types such as <c>NSWindowController</c>, <c>NSViewController</c>,
	/// etc.
	/// </summary>
	/// <returns>The main top level element from the NIB as a known type such as 
	/// <c>NSWindowController</c>, <c>NSViewController</c>, etc.</returns>
	/// <param name="nibName">The name of the NIB to load.</param>
	private NSObject LoadNib (string nibName)
	{
		NSObject mainElement = null;
		var topLevelObjects = new NSArray ();
		NSObject element = null;
		var subNibName = "";

		// Anything to process?
		if (nibName == null) return null;

		//Console.WriteLine ("Loading {0} -->", nibName);

		// Load the given NIB
		if (Bundle.LoadNibNamed (nibName, FirstResponder, out topLevelObjects)) {
			// Discovery
			//Console.WriteLine ("Parsing {0}:", nibName);
			//PrintTopObjects (topLevelObjects);

			// Scan all top level object for a known object type
			for (nuint n = 0; n < topLevelObjects.Count; ++n) {
				// Access the current object
				element = topLevelObjects.GetItem<NSObject> (n);

				// Search for known types
				if (element is NSMenu) {
					// Found main menu
					if (mainElement == null) mainElement = element;
					var menu = element as NSMenu;
					LoadMenuSegueTargets (menu);
				} else if (element is NSWindowController) {
					// Found window controller 
					mainElement = element;
					var windowController = element as NSWindowController;

					// Load the window's content view
					if (windowController.Window.ContentViewController is NSSplitViewController) {
						// Access Split View Controller
						var splitViewController = windowController.Window.ContentViewController as NSSplitViewController;

						// Manufacture a new Split View and configure it the same as
						// the Split View from the storyboard
						var splitView = new NSSplitView (splitViewController.View.Frame) {
							ArrangesAllSubviews = splitViewController.SplitView.ArrangesAllSubviews,
							DividerStyle = splitViewController.SplitView.DividerStyle,
							IsVertical = splitViewController.SplitView.IsVertical
						};

						// Load all the attached views
						foreach (NSSplitViewItem svItem in splitViewController.SplitViewItems) {
							subNibName = svItem.ViewController.NibName;
							var view = LoadNib (subNibName) as NSView;
							splitView.AddArrangedSubview (view);
						}
						splitView.AdjustSubviews ();

						// Attach to window
						windowController.Window.ContentView = splitView;

					} else {
						subNibName = windowController.Window.ContentViewController.NibName;
						var contentView = LoadNib (subNibName) as NSView;

						// Found?
						if (contentView != null) {
							// Yes, attach it to the window
							windowController.Window.ContentViewController.View = contentView;
							windowController.Window.ContentView = contentView;
							//Console.WriteLine ("* {0} View Attached", subNibName);
						}
					}

					// Is there a toolbar?
					if (windowController.Window.Toolbar != null) {
						// Scan items for attached segues
						LoadToolbarItemSegueTargets (windowController.Window.Toolbar);
					}
				} else if (element is NSViewController) {
					// Found View Controller
					if (mainElement == null) mainElement = element;
					var viewController = element as NSViewController;

					// Load the controller's view
					subNibName = viewController.NibName;
					var view = LoadNib (subNibName) as NSView;
					viewController.View = view;
				} else if (element is NSView) {
					// Found View
					if (mainElement == null) mainElement = element;
					var view = element as NSView;
					LoadViewSegueTargets (view);
				}
			}
		} else {
			// Report error
			Console.WriteLine ("Unable to load: {0}", nibName);
		}

		// Return the found main element
		return mainElement;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Instantiates the main menu from the storyboard.
	/// </summary>
	/// <returns>The main menu.</returns>
	public NSMenu InstantiateMainMenu ()
	{
		return LoadNib (MainMenuIdentifier) as NSMenu;
	}

	/// <summary>
	/// Instantiates the main menu from the storyboard and binds it to the
	/// given instance of a workbook class.
	/// </summary>
	/// <returns>The main menu.</returns>
	/// <param name="parent">The workbook instance to bind to.</param>
	public NSMenu InstantiateMainMenu (NSObject parent)
	{
		// Inflate menu
		var menu = LoadNib (MainMenuIdentifier) as NSMenu;

		// Bind menu to parent class
		StoryboardBinder.BindMenu (menu, parent);

		// return inflated menu
		return menu;
	}

	/// <summary>
	/// Instantiates the initial controller as specified in the Storyboard.
	/// </summary>
	/// <returns>The initial controller.</returns>
	public NSObject InstantiateInitialController ()
	{
		// Load element
		var element = LoadNib (EntryPointIdentifier);

		// Is this a window controller?
		if (element is NSWindowController) {
			// Yes, display the window to the user
			var windowController = element as NSWindowController;
			windowController.Window.MakeKeyAndOrderFront ((NSObject)NSApplication.SharedApplication.Delegate);
		}

		// Return loaded object
		return element;
	}

	/// <summary>
	/// Instantiates the initial controller as specified in the Storyboard and binds it to
	/// the given instance of a workbook class.
	/// </summary>
	/// <returns>The initial controller.</returns>
	/// <param name="parent">The workbook instance to bind to.</param>
	public NSObject InstantiateInitialController (NSObject parent)
	{
		// Get initial element
		var element = InstantiateInitialController ();

		// Bind element to parent class
		StoryboardBinder.Bind (element, parent);

		// Return loaded object
		return element; 
	}

	/// <summary>
	/// Instantiates the controller from the NIB of the given name.
	/// </summary>
	/// <returns>The controller for nib name.</returns>
	/// <param name="nibName">Nib name.</param>
	public NSObject InstantiateControllerForNibName (string nibName)
	{
		var element = LoadNib (nibName);

		// Return loaded object
		return element;
	}

	/// <summary>
	/// Returns the NIB name for the given Identifier or the empty string ("") if
	/// not found.
	/// </summary>
	/// <returns>The name for identifier or empty string ("") if not found.</returns>
	/// <param name="identifier">The identifier to search for.</param>
	public string NibNameForIdentifier (string identifier)
	{
		for (int n = 0; n < ViewControllerIdentifiersToNibNames.Keys.Count (); ++n) {
			// Get the key and value
			var key = ViewControllerIdentifiersToNibNames.Keys [n].ToString ();
			var value = ViewControllerIdentifiersToNibNames.Values [n].ToString ();

			// Found?
			if (key == identifier) return value;
		}

		// Return found name
		return "";
	}

	/// <summary>
	/// Returns the NIB name for the partial Identifier or the empty string ("") if
	/// not found.
	/// </summary>
	/// <returns>The name for identifier or empty string ("") if not found.</returns>
	/// <param name="identifier">The partial identifier to search for.</param>
	public string NibNameForPartialIdentifier (string identifier)
	{
		for (int n = 0; n < ViewControllerIdentifiersToNibNames.Keys.Count (); ++n) {
			// Get the key and value
			var key = ViewControllerIdentifiersToNibNames.Keys [n].ToString ();
			var value = ViewControllerIdentifiersToNibNames.Values [n].ToString ();

			// Found?
			if (key.Contains(identifier)) return value;
		}

		// Return found name
		return "";
	}

	/// <summary>
	/// Returns the UUID for the given Identifier or the empty string ("") if
	/// not found.
	/// </summary>
	/// <returns>The name for identifier or empty string ("") if not found.</returns>
	/// <param name="identifier">The identifier to search for.</param>
	public string UUIDForIdentifier (string identifier)
	{
		for (int n = 0; n < ViewControllerIdentifiersToUUIDs.Keys.Count (); ++n) {
			// Get the key and value
			var key = ViewControllerIdentifiersToUUIDs.Keys [n].ToString ();
			var value = ViewControllerIdentifiersToUUIDs.Values [n].ToString ();

			// Found?
			if (key == identifier) return value;
		}

		// Return found name
		return "";
	}

	/// <summary>
	/// Instantiates the controller for the given identifier.
	/// </summary>
	/// <returns>The controller for identifier.</returns>
	/// <param name="identifier">The identifier to instantiate.</param>
	public NSObject InstantiateControllerForIdentifier (string identifier)
	{
		var nibName = NibNameForIdentifier (identifier);

		// Found?
		if (nibName == "") {
			// No
			return null;
		} else {
			// Yes, instantiate named nib
			return InstantiateControllerForNibName (nibName);
		}
	}

	/// <summary>
	/// Instantiates the controller for the given partial identifier.
	/// </summary>
	/// <returns>The controller for partial identifier.</returns>
	/// <param name="identifier">The partial identifier to instantiate.</param>
	public NSObject InstantiateControllerForPartialIdentifier (string identifier)
	{
		var nibName = NibNameForPartialIdentifier (identifier);

		// Found?
		if (nibName == "") {
			// No
			return null;
		} else {
			// Yes, instantiate named nib
			return InstantiateControllerForNibName (nibName);
		}
	}

	/// <summary>
	/// Finds the segue definition for the given Scene and Source Object.
	/// </summary>
	/// <returns>The segue definition or <c>null</c> if not found.</returns>
	/// <param name="sceneType">Scene type.</param>
	/// <param name="sceneID">Scene identifier.</param>
	/// <param name="sourceObjectType">Source object type.</param>
	/// <param name="sourceObjectID">Source object identifier.</param>
	public StoryboardSegueDefinition FindSegueDefinition (StoryboardObjectType sceneType, string sceneID, StoryboardObjectType sourceObjectType, string sourceObjectID)
	{
		// Scan all definitions
		foreach (StoryboardSegueDefinition definition in SegueDefinitions) {
			// Found?
			if (definition.SceneType == sceneType &&
			    definition.SceneID == sceneID &&
				definition.SourceObjectType == sourceObjectType && 
			    definition.SourceObjectID == sourceObjectID) {
				return definition;
			}
		}

		// Not found
		return null;
	}

	/// <summary>
	/// Finds the segue definition for the given Source Object.
	/// </summary>
	/// <returns>The segue definition or <c>null</c> if not found</returns>
	/// <param name="sourceObjectType">Source object type.</param>
	/// <param name="sourceObjectID">Source object identifier.</param>
	public StoryboardSegueDefinition FindSegueDefinition (StoryboardObjectType sourceObjectType, string sourceObjectID)
	{
		// Scan all definitions
		foreach (StoryboardSegueDefinition definition in SegueDefinitions) {
			// Found?
			if (definition.SourceObjectType == sourceObjectType && definition.SourceObjectID == sourceObjectID) {
				return definition;
			}
		}

		// Not found
		return null;
	}

	/// <summary>
	/// Finds the segue definition for the given Segue Identifier.
	/// </summary>
	/// <returns>The segue definition or <c>null</c> if not found.</returns>
	/// <param name="identifier">The Segue Identifier to find.</param>
	public StoryboardSegueDefinition FindSegueDefinition (string identifier)
	{
		// Scan all definitions
		foreach (StoryboardSegueDefinition definition in SegueDefinitions) {
			// Found?
			if (definition.SegueIdentifier == identifier) {
				return definition;
			}
		}

		// Not found
		return null;
	}
	#endregion
}
