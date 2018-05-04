#r "Xamarin.Forms.Core"
#r "Xamarin.Forms.Xaml"
#r "Xamarin.Forms.Platform"
#r "SQLite-net"


using System;
using Xamarin.Forms;
using SQLite;
using SQLitePCL;

var page = new ContentPage();
StackLayout stackLayout = new StackLayout { Margin = new Thickness(0, 20, 0, 0) };
page.Content = stackLayout;
Application.Current.MainPage = page;