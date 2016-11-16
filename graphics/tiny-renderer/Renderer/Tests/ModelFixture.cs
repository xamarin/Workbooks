using System;

using NUnit.Framework;

namespace Renderer
{
	static class ModelUtils
	{
		const string text = @"
v 0 0 0
v 0 1 0
v 1 0 0

vn 0 0 1
vn 0 0 2
vn 0 0 3

f 1/-1/1 2/-1/2 3/-1/3
";

		public static Model GetModel1 ()
		{
			return Model.FromText (text);
		}
	}

	[TestFixture]
	public class ModelFixture
	{
		[Test]
		public void Normal ()
		{
			var model = ModelUtils.GetModel1 ();
			var face = model.Faces [0];

			var n1 = model.Normal (face, 0);
			Assert.AreEqual (1, n1.z);

			var n2 = model.Normal (face, 1);
			Assert.AreEqual (2, n2.z);

			var n3 = model.Normal (face, 2);
			Assert.AreEqual (3, n3.z);
		}
	}
}
