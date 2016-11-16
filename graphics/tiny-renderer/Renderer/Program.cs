using System;
using static Renderer.Geometry;
using static Renderer.Utils;
using static Renderer.MatrixHelpers;

namespace Renderer
{
	class MainClass
	{
		static int width = 800, height = 800;

		static readonly Image gridTexture = Image.Load ("../../../obj/grid.tga");
#region head
		static readonly Model headModel = Model.FromFile ("../../../obj/african_head.obj");
		static readonly Image headTexture = Image.Load ("../../../obj/african_head_diffuse.tga");
		static readonly Image headNormalMap = Image.Load ("../../../obj/african_head_nm.tga");
		static readonly Image headTangentMap = Image.Load ("../../../obj/african_head_nm_tangent.tga");
		static readonly Image headSpecularMap = Image.Load ("../../../obj/african_head_spec.tga");
#endregion

#region diablo
		static readonly Model diabloModel = Model.FromFile ("../../../obj/diablo3_pose.obj");
		static readonly Image diabloTexture = Image.Load ("../../../obj/diablo3_pose_diffuse.tga");
		static readonly Image diabloNormalMap = Image.Load ("../../../obj/diablo3_pose_nm.tga");
		static readonly Image diabloTangentMap = Image.Load ("../../../obj/diablo3_pose_nm_tangent.tga");
		static readonly Image diabloSpecularMap = Image.Load ("../../../obj/diablo3_pose_spec.tga");

		// Lesson 8
		static readonly Image aoTexture = Image.Load ("../../../obj/diablo3-total-occlusion.tga");
#endregion

		static readonly Vec3f[] world = new Vec3f [3];
		static readonly Vec3f[] screen = new Vec3f [3];
		static readonly Vec2f[] uv = new Vec2f [3];
		static readonly Vec3f light_dir = new Vec3f { x = 1, y = 1, z = 1 };
		static readonly Vec3f eye = new Vec3f { x = 1, y = 1, z = 3 };
		static readonly Vec3f center = new Vec3f { x = 0, y = 0, z = 0 };
		static readonly Vec3f up = new Vec3f { x = 0, y = 1, z = 0 };

		static readonly Matrix4 modelView = LookAt (eye, center, up);
		static readonly Matrix4 viewPort = Viewport (width / 8, height / 8, width * 3 / 4, height * 3 / 4);
		static readonly Matrix4 projection = Projection (-1f / (eye - center).Norm ());

		static readonly Func<Vec3f, Vec3f> map = v => {
			var t = viewPort * projection * modelView;
			var r = Project3D (Mult (t, Embed4D (v)));
			r.x = (int)Math.Round (r.x, MidpointRounding.AwayFromZero);
			r.y = (int)Math.Round (r.y, MidpointRounding.AwayFromZero);
			return r;
		};

		static readonly Func<Vec3f, Vec2f> uvMap = v => new Vec2f {
			x = v.x * (headTexture.Width - 1),
			y = v.y * (headTexture.Height - 1)
		};

		public static void Main (string [] args)
		{
			headTexture.VerticalFlip ();
			headNormalMap.VerticalFlip ();
			headTangentMap.VerticalFlip ();
			headSpecularMap.VerticalFlip ();

			diabloTexture.VerticalFlip ();
			diabloNormalMap.VerticalFlip ();
			diabloTangentMap.VerticalFlip ();
			diabloSpecularMap.VerticalFlip ();

			// lesson 8
			aoTexture.VerticalFlip ();

			//LightListing ();
			//ListingZBuffer.Execute (model);
			//ListingTexture.Execute (model, texture);
			//ProjectionListing ();
			//CameraMoveListing ();
			//GouraudShaderListing ();
			//GouraudShader6Listing ();
			//TextureShaderListing ();
			//NormalMapListing ();
			//SpecularShaderListing ();
			//TangetShaderListing ();

			DepthShaderListing ();
			//AmbientListing ();
			//AOShaderListing ();

			//ScreenSpaceAOListing ();
		}

		static void ScreenSpaceAOListing ()
		{
			//var model = headModel;
			var model = diabloModel;

			var shader = new ZShader2 (model, viewPort, projection, modelView);
			var result = Render (model, shader);
			var image = result.Image;
			var zbuffer = result.ZBuffer;

			for (int x = 0; x < image.Width; x++) {
				for (int y = 0; y < image.Height; y++) {
					if (zbuffer [x + y * image.Width] < -1e5)
						continue;

					double total = 0;
					for (var a = 0.0; a < 2 * Math.PI - 0.001; a += Math.PI / 4) {
						total += Math.PI / 2 - MaxElevationAngle (zbuffer,
						                                          new Vec2f { x = x, y = y },
						                                          new Vec2f { x = (float)Math.Cos (a), y = (float)Math.Sin (a) },
						                                          image.Width, image.Height);
					}
					total /= (Math.PI / 2) * 8;
					var v = (byte)(Math.Min (255, total * 255));
					image [x, y] = new Color (v, v, v);
				}
			}

			image.VerticalFlip ();
			image.WriteToFile ("ao-screen-space.tga");
		}

		static void AOShaderListing ()
		{
			var shader = new AOShader (diabloModel, viewPort, projection, modelView, aoTexture);
			Render (diabloModel, shader, "ao-shader.tga");
		}

		static void AmbientListing ()
		{
			var rnd = new Random ();
			var screen_coords = new Vec4f [3];

			var frame = new Image (width, height, Format.BGR);
			var shadowbuffer = InitZBuffer (frame);
			var zbuffer = InitZBuffer (frame);

			var total = new Image (1024, 1024, Format.BGR);
			var occl = new Image (1024, 1024, Format.BGR);

			const int nrenders = 1;
			for (int iter = 1; iter <= nrenders; iter++) {
				for (int i = 0; i < shadowbuffer.Length; i++) {
					shadowbuffer [i] = 0;
					zbuffer [i] = 0;
				}

				var vUp = new Vec3f {
					x = (float)rnd.NextDouble (),
					y = (float)rnd.NextDouble (),
					z = (float)rnd.NextDouble ()
				};
				var spLocation = RandPointOnUnitSphere ();
				spLocation.y = Math.Abs (spLocation.y);

				frame.Clear ();
				var mvM = LookAt (spLocation, center, vUp);
				var pM = Projection (0);
				var zshader = new ZShader (diabloModel, viewPort, pM, mvM);
				foreach (var face in diabloModel.Faces) {
					for (int i = 0; i < 3; i++)
						screen_coords [i] = zshader.Vertex (face, i);
					Triangle (frame, screen_coords, zshader, shadowbuffer);
				}
				//frame.VerticalFlip ();
				//frame.WriteToFile ("framebuffer.tga");

				occl.Clear ();
				var shader = new OcclusionShader (diabloModel, viewPort, pM, mvM, occl, shadowbuffer, frame.Width);
				foreach (var face in diabloModel.Faces) {
					for (int i = 0; i < 3; i++)
						screen_coords [i] = shader.Vertex (face, i);
					Triangle (frame, screen_coords, shader, zbuffer);
				}

				for (int i = 0; i < total.Width; i++) {
					for (int j = 0; j < total.Height; j++) {
						float prev = total [i, j] [0];
						float curr = occl [i, j] [0];
						var val = (byte)((prev * (iter - 1) + curr) / iter + 0.5f);
						total [i, j] = new Color (val, val, val);
					}
				}
			}

			total.VerticalFlip ();
			total.WriteToFile ("total-occlusion.tga");
		}

		static void DepthShaderListing ()
		{
			var model = diabloModel;

			var M = viewPort * Projection (0) * LookAt (light_dir, center, up);
			var depthShader = new DepthShader (model, M);
			var step1 = Render (model, depthShader, "diablo-depth-shader.tga");

			var shadowM = M * Inverse (viewPort * projection * modelView);
			var shader = new ShadowShader (model, viewPort, projection, modelView, shadowM, light_dir,
			                               diabloTexture, diabloNormalMap, diabloSpecularMap,
			                               step1.ZBuffer, width);
			Render (model, shader, "diablo-shadow-shader.tga");
		}

		static void TangetShaderListing ()
		{
			var model = headModel;
			var shader = new TangentShader (model, viewPort, projection, modelView, light_dir, headTexture /*gridTexture*/, headTangentMap /*normalMap*/);
			Render (model, shader, "tanget-shader.tga");
		}

		static void SpecularShaderListing ()
		{
			RunSpec (headModel, headTexture, headNormalMap, headSpecularMap, "specular-shader.tga");
			RunSpec (diabloModel, diabloTexture, diabloNormalMap, diabloSpecularMap, "diablo-specular-shader.tga");
		}

		static void RunSpec (Model model, Image texture, Image nm, Image sm, string path)
		{
			var shader = new SpecularShader (model, viewPort, projection, modelView, light_dir, texture, nm, sm);
			Render (model, shader, path);
		}

		static void NormalMapListing ()
		{
			var shader = new NormalMapShader (headModel, viewPort, projection, modelView, light_dir, headTexture, headNormalMap);
			Render (headModel, shader, "normal-map-shader.tga");
		}

		static void TextureShaderListing ()
		{
			var shader = new TextureShader (headModel, viewPort, projection, modelView, light_dir, headTexture);
			Render (headModel, shader, "texture-shader.tga");
		}

		static void GouraudShader6Listing ()
		{
			var shader = new GouraudShader6 (headModel, viewPort, projection, modelView, light_dir);
			Render (headModel, shader, "gouraud-shader6.tga");
		}

		static void GouraudShaderListing ()
		{
			var shader = new GouraudShader (headModel, viewPort, projection, modelView, light_dir);
			Render (headModel, shader, "gouraud-shader.tga");
		}

		static RenderResult Render (Model model, IShader shader, string path)
		{
			var result = Render (model, shader);
			result.Image.VerticalFlip ();
			result.Image.WriteToFile (path);

			return result;
		}

		static RenderResult Render (Model model, IShader shader)
		{
			var image = new Image (width, height, Format.BGR);
			var zbuffer = InitZBuffer (image);
			var screen_coords = new Vec4f [3];

			foreach (var face in model.Faces) {
				for (int i = 0; i < 3; i++)
					screen_coords [i] = shader.Vertex (face, i);
				Triangle (image, screen_coords, shader, zbuffer);
			}

			return new RenderResult {
				Image = image,
				ZBuffer = zbuffer
			};
		}

		class RenderResult
		{
			public Image Image { get; set; }
			public float [] ZBuffer { get; set; }
		}

		static void CameraMoveListing ()
		{
			var image = new Image (width, height, Format.BGR);

			var zbuffer = InitZBuffer (image);
			foreach (var face in headModel.Faces) {
				for (int i = 0; i < 3; i++) {
					var vIndex = face.Vertices [i];
					world [i] = headModel.Vertices [vIndex];
					screen [i] = map (world [i]);

					var tIndex = face.Textures [i];
					uv [i] = uvMap (headModel.Textures [tIndex]);
				}

				Vec3f n = Cross (world [2] - world [0], world [1] - world [0]).Normalize ();

				var intensivity = Dot (n, light_dir);
				if (intensivity > 0)
					Triangle (image, screen, headTexture, uv, intensivity, zbuffer);
			}

			image.VerticalFlip ();
			image.WriteToFile ("camera-moved.tga");
		}

		static void ProjectionListing ()
		{
			var image = new Image (width, height, Format.BGR);

			var camera = new Vec3f { z = 3 };
			var projection = Projection (-1 / camera.z);
			var viewPort = Viewport (width / 8, height / 8, width * 3 / 4, height * 3 / 4);

			var zbuffer = InitZBuffer (image);
			foreach (var face in headModel.Faces) {
				for (int i = 0; i < 3; i++) {
					var vIndex = face.Vertices [i];
					world [i] = headModel.Vertices [vIndex];
					screen [i] = map (world [i]);

					var tIndex = face.Textures [i];
					uv [i] = uvMap (headModel.Textures [tIndex]);
				}

				var n = Cross (world [2] - world [0], world [1] - world [0]).Normalize ();

				var intensivity = Dot (n, light_dir);
				if (intensivity > 0)
					Triangle (image, screen, headTexture, uv, intensivity, zbuffer);
			}

			image.VerticalFlip ();
			image.WriteToFile ("project-head.tga");
		}

		static void LightListing ()
		{
			var image = new Image (width, height, Format.BGR);
			Func<Vec3f, Vec3f> map = v => new Vec3f { x = (int)((v.x + 1) * image.Width / 2), y = (int)((v.y + 1) * image.Height / 2) };

			var light_dir = new Vec3f { x = 0, y = 0, z = -1 };

			foreach (var face in headModel.Faces) {
				for (int i = 0; i < 3; i++) {
					var vIndex = face.Vertices [i];
					world [i] = headModel.Vertices [vIndex];
					screen [i] = map (world [i]);
				}

				var n = Cross (world [2] - world [0], world [1] - world [0]).Normalize ();

				var intensivity = Dot (n, light_dir);
				if (intensivity > 0)
					Triangle (image, screen, Color.White * intensivity);
			}

			image.VerticalFlip ();
			image.WriteToFile ("gray-head.tga");
		}

		static void TriangleListing ()
		{
			var image = new Image (200, 200, Format.BGR);
			Vec3f [] coordinates = {
				new Vec3f {x=10,  y=10},
				new Vec3f {x=100, y=30},
				new Vec3f {x=190, y=160}
			};
			Triangle (image, coordinates, Color.Red);

			image.VerticalFlip ();
			image.WriteToFile ("red-triangle.tga");
		}
	}
}
