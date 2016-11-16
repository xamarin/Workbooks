using System;
using NUnit.Framework;

using static Renderer.Utils;
using static Renderer.MatrixHelpers;
using static Renderer.Geometry;

namespace Renderer
{
	[TestFixture]
	public class GouraudShaderFixture
	{
		static int width = 800, height = 800;
		static readonly Vec3f eye = new Vec3f { x = 1, y = 1, z = 3 };
		static readonly Vec3f center = new Vec3f { x = 0, y = 0, z = 0 };
		static readonly Vec3f up = new Vec3f { x = 0, y = 1, z = 0 };

		[Test]
		public void Vertex ()
		{
			var model = ModelUtils.GetModel1 ();

			var ligthdir = new Vec3f { z = 3 };

			var I = Matrix4.Identity ();
			//var viewPort = I;
			//var projection = I;
			//var modelView = I;
			var modelView = LookAt (eye, center, up);
			var viewPort = Viewport (width / 8, height / 8, width * 3 / 4, height * 3 / 4);
			var projection = Projection (-1f / (eye - center).Norm ());

			var shader = new GouraudShader (model, viewPort, projection, modelView, ligthdir);

			var face = model.Faces [0];
			var sc0 = shader.Vertex (face, 0);
			//sc0 = sc0 / sc0.h;

			var sc1 = shader.Vertex (face, 1);
			//sc1 = sc1 / sc1.h;

			var sc2 = shader.Vertex (face, 2);
			//sc2 = sc2 / sc2.h;

			Func<Vec3f, Vec3f> map = v => {
				var transformation = viewPort * projection * modelView;
				var r = Project3D (Mult (transformation, Embed4D (v)));
				//r.x = (int)Math.Round (r.x, MidpointRounding.AwayFromZero);
				//r.y = (int)Math.Round (r.y, MidpointRounding.AwayFromZero);
				return r;
			};

			var v0 = model.Vertices [face.Vertices [0]];
			var v1 = model.Vertices [face.Vertices [1]];
			var v2 = model.Vertices [face.Vertices [2]];

			var t0 = map (v0);
			var t1 = map (v1);
			var t2 = map (v2);

			Assert.True (Math.Abs (sc0.x - t0.x) < 0.0001f);
			Assert.True (Math.Abs (sc0.y - t0.y) < 0.0001f);
			Assert.True (Math.Abs (sc0.z - t0.z) < 0.0001f);

			Assert.True (Math.Abs (sc1.x - t1.x) < 0.0001f);
			Assert.True (Math.Abs (sc1.y - t1.y) < 0.0001f);
			Assert.True (Math.Abs (sc1.z - t1.z) < 0.0001f);

			Assert.True (Math.Abs (sc2.x - t2.x) < 0.0001f);
			Assert.True (Math.Abs (sc2.y - t2.y) < 0.0001f);
			Assert.True (Math.Abs (sc2.z - t2.z) < 0.0001f);


		}
	}
}
