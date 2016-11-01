//Alert with One Button
var okayButton = UIButton.FromType (UIButtonType.System);
okayButton.Frame = new CGRect (10, 30, 300, 40);
okayButton.SetTitle ("Okay Button", UIControlState.Normal);

//Alert with Two Buttons
var okayCancelButton = UIButton.FromType (UIButtonType.System);
okayCancelButton.Frame = new CGRect (10, 80, 300, 40);
okayCancelButton.SetTitle ("Okay / Cancel Button", UIControlState.Normal);

//Text Input Alert
var textInputButton = UIButton.FromType (UIButtonType.System);
textInputButton.Frame = new CGRect (10, 130, 300, 40);
textInputButton.SetTitle ("Text Input", UIControlState.Normal);

var actionSheetButton = UIButton.FromType (UIButtonType.System);
actionSheetButton.Frame = new CGRect (10, 180, 300, 40);
actionSheetButton.SetTitle ("Action Sheet", UIControlState.Normal);

RootViewController.View.AddSubview(okayButton);
RootViewController.View.AddSubview(okayCancelButton);
RootViewController.View.AddSubview(textInputButton);
RootViewController.View.AddSubview(actionSheetButton);

