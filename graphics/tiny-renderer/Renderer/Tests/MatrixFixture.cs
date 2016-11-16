using System;

using NUnit.Framework;

using static Renderer.MatrixHelpers;

namespace Renderer
{
	[TestFixture]
	public class MatrixFixture
	{
		[Test]
		public void InverseIdentity ()
		{
			var identity = Matrix4.Identity ();
			var ti = TransposeInverse (identity);

			Console.WriteLine (ti);
		}
	}
}
