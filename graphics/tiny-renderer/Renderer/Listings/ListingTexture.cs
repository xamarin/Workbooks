using System;
using static Renderer.Geometry;
using static Renderer.Utils;

namespace Renderer
{
	static class ListingTexture
	{
		static int width = 800, height = 800;
		static readonly Vec3f [] world = new Vec3f [3];
		static readonly Vec3f [] screen = new Vec3f [3];
		static readonly Vec3f light_dir = new Vec3f { x = 0, y = 0, z = -1 };

		public static void Execute (Model model, Image texture)
		{
			var image = new Image (width, height, Format.BGR);
			Func<Vec3f, Vec3f> map = v => new Vec3f {
				x = (int)Math.Round ((v.x + 1) * (image.Width - 1) / 2, MidpointRounding.AwayFromZero),
				y = (int)Math.Round ((v.y + 1) * (image.Height - 1) / 2, MidpointRounding.AwayFromZero),
				z = v.z
			};

			Func<Vec3f, Vec2f> uvMap = v => new Vec2f {
				x = v.x * (texture.Width - 1),
				y = v.y * (texture.Height - 1)
			};

			var zbuffer = InitZBuffer (image);
			var uv = new Vec2f [3];
			foreach (var face in model.Faces) {
				for (int i = 0; i < 3; i++) {
					var vIndex = face.Vertices [i];
					world [i] = model.Vertices [vIndex];
					screen [i] = map (world [i]);

					var tIndex = face.Textures [i];
					uv [i] = uvMap (model.Textures [tIndex]);
				}

				Vec3f n = Cross (world [2] - world [0], world [1] - world [0]).Normalize ();

				var intensivity = Dot (n, light_dir);
				if (intensivity > 0)
					Triangle (image, screen, texture, uv, intensivity, zbuffer);
			}

			image.VerticalFlip ();
			image.WriteToFile ("texture-head.tga");
		}
	}
}
