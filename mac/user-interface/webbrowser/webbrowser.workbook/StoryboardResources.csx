using System;
using System.Diagnostics;
using System.IO;
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
// LIMITATIONS: Actions and Segues are currently not supported. For Actions,
// use the Activated Event of a NSControl bound to an "Outlet". For Segues,
// the StoryboardInflator can inflate and populate any NIB that will then
// need to be manually displayed.

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
/// Actions and Segues are currently not supported. For many standard items (such as
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
		if (workbookClass is NSWindow && workbookClass.GetType().Name == window.Identifier) {
			// Yes, bind objects
			var wbWindow = workbookClass as NSWindow;
			wbWindow.ContentViewController = window.ContentViewController;
			wbWindow.ContentView = window.ContentView;
		}

		// Bind content view controller
		BindViewController (window.ContentViewController, workbookClass);
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

		// Load the given NIB
		if (Bundle.LoadNibNamed (nibName, FirstResponder, out topLevelObjects)) {
			// Discovery
			//Console.WriteLine ("Loading {0}:", nibName);
			//PrintTopObjects (topLevelObjects);

			// Scan all top level object for a known object type
			for (nuint n = 0; n < topLevelObjects.Count; ++n) {
				// Access the current object
				element = topLevelObjects.GetItem<NSObject> (n);

				// Search for known types
				if (element is NSMenu) {
					// Found main menu
					mainElement = element;
				} else if (element is NSWindowController) {
					// Found window controller 
					mainElement = element;
					var windowController = element as NSWindowController;

					// Load the window's content view
					subNibName = windowController.Window.ContentViewController.NibName;
					var contentView = LoadNib (subNibName) as NSView;

					// Found?
					if (contentView != null) {
						// Yes, attach it to the window
						windowController.Window.ContentViewController.View = contentView;
						windowController.Window.ContentView = contentView;
						//Console.WriteLine ("* {0} View Attached", subNibName);
					}
				} else if (element is NSViewController) {
					// Found View Controller
					mainElement = element;
					var viewController = element as NSViewController;

					// Load the controller's view
					subNibName = viewController.NibName;
					var view = LoadNib (subNibName) as NSView;
				} else if (element is NSView) {
					// Found View
					mainElement = element;
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
	#endregion
}
