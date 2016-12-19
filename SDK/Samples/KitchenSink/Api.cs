using System;

using Xamarin.Interactive.Representations;

public static class KitchenSink
{
	static readonly Random random = new Random ();

	public static Color RandomColor ()
		=> new Color (random.NextDouble (), random.NextDouble (), random.NextDouble ());
}