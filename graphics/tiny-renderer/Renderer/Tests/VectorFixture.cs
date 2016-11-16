using System;

using NUnit.Framework;

namespace Renderer
{
	[TestFixture]
	public class VectorFixture
	{
		[Test]
		public void Project3D ()
		{
			var v4 = new Vec4f { x = 10, y = 11, z = 12, h = 13 };
			var v3 = Geometry.Project3D (v4);

			Assert.AreEqual (v4.x, v3.x);
			Assert.AreEqual (v4.y, v3.y);
			Assert.AreEqual (v4.z, v3.z);
		}

		[Test]
		public void Embed4D_DefaultValue ()
		{
			var v3 = new Vec3f { x = 10, y = 11, z = 12 };
			var v4 = Geometry.Embed4D (v3);

			Assert.AreEqual (v3.x, v4.x);
			Assert.AreEqual (v3.y, v4.y);
			Assert.AreEqual (v3.z, v4.z);
			Assert.AreEqual (1, v4.h);
		}

		[Test]
		public void Embed4D ()
		{
			var v3 = new Vec3f { x = 10, y = 11, z = 12 };
			var v4 = Geometry.Embed4D (v3, 777);

			Assert.AreEqual (v3.x, v4.x);
			Assert.AreEqual (v3.y, v4.y);
			Assert.AreEqual (v3.z, v4.z);
			Assert.AreEqual (777, v4.h);
		}

		[Test]
		public void VectorIndexer ()
		{
			float x = 10f, y = 11f, z = 12f, h = 13f;

			var v4 = new Vec4f ();
			v4 [0] = x;
			v4 [1] = y;
			v4 [2] = z;
			v4 [3] = h;

			Assert.AreEqual (x, v4 [0]);
			Assert.AreEqual (y, v4 [1]);
			Assert.AreEqual (z, v4 [2]);
			Assert.AreEqual (h, v4 [3]);

			v4 = new Vec4f ();
			v4.x = x;
			v4.y = y;
			v4.z = z;
			v4.h = h;

			Assert.AreEqual (x, v4.x);
			Assert.AreEqual (y, v4.y);
			Assert.AreEqual (z, v4.z);
			Assert.AreEqual (h, v4.h);
		}

		[Test]
		public void Vector4_Divide ()
		{
			var v4 = new Vec4f { x = 10, y = 20, z = 30, h = 40 };
			var r4 = v4 / 10;

			Assert.AreEqual (1, r4.x);
			Assert.AreEqual (2, r4.y);
			Assert.AreEqual (3, r4.z);
			Assert.AreEqual (4, r4.h);
		}

		[Test]
		public void Vector4_Sub ()
		{
			var a4 = new Vec4f { x = 10, y = 20, z = 30, h = 40 };
			var b4 = new Vec4f { x = 1,  y = 2,  z = 3,  h = 4 };

			var r = a4 - b4;
			Assert.AreEqual (9, r.x);
			Assert.AreEqual (18, r.y);
			Assert.AreEqual (27, r.z);
			Assert.AreEqual (36, r.h);
		}

		[Test]
		public void Vector4_Norm ()
		{
			var b4 = new Vec4f { x = 1, y = 2, z = 3, h = 4 };
			var len = b4.Norm ();
			Assert.True (Math.Abs (Math.Sqrt (30) - len) <= 0.001);
		}

		[Test]
		public void Vector4_Normalize ()
		{
			var b4 = new Vec4f { x = 1, y = 2, z = 3, h = 4 };
			var len = b4.Norm ();
			var n = b4.Normalize ();
			Assert.True (Math.Abs (n.x - 1f / len) <= 0.001);
			Assert.True (Math.Abs (n.y - 2f / len) <= 0.001);
			Assert.True (Math.Abs (n.z - 3f / len) <= 0.001);
			Assert.True (Math.Abs (n.h - 4f / len) <= 0.001);
		}
	}
}
