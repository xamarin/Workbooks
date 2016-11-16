using System;

using static Renderer.Geometry;

namespace Renderer
{
	static class Utils
	{
		public static Vec3f Barycentric (Vec3f a, Vec3f b, Vec3f c, Vec2i p)
		{
			var pixel = new Vec3f { x = p.x + 0.5f, y = p.y + 0.5f };

			var ab = b - a;
			var ac = c - a;
			var pa = a - pixel;

			var r = Cross (new Vec3f { x = ab.x, y = ac.x, z = pa.x }, new Vec3f { x = ab.y, y = ac.y, z = pa.y });

			// triangle is degenerate, in this case return smth with negative coordinates 
			if (Math.Abs (r.z) < 1)
				return new Vec3f { x = -1, y = 1, z = 1 };

			return new Vec3f { x = 1 - r.x / r.z - r.y / r.z, y = r.x / r.z, z = r.y / r.z };
		}

		public static void Triangle (Image image, Vec3f [] coordinates, Color color)
		{
			var t0 = coordinates [0];
			var t1 = coordinates [1];
			var t2 = coordinates [2];

			var pMax = new Vec2i {
				x = (int)Math.Max (0, Math.Max (t0.x, Math.Max (t1.x, t2.x))),
				y = (int)Math.Max (0, Math.Max (t0.y, Math.Max (t1.y, t2.y)))
			};
			var pMin = new Vec2i {
				x = (int)Math.Min (image.Width, Math.Min (t0.x, Math.Min (t1.x, t2.x))),
				y = (int)Math.Min (image.Height, Math.Min (t0.y, Math.Min (t1.y, t2.y)))
			};

			Vec2i p;
			for (p.x = pMin.x; p.x <= pMax.x; p.x++) {
				for (p.y = pMin.y; p.y <= pMax.y; p.y++) {
					var bc = Barycentric (t0, t1, t2, p);
					if (bc.x < 0 || bc.y < 0 || bc.z < 0)
						continue;
					image [p.x, p.y] = color;
				}
			}
		}

		// lesson 3
		public static Vec3f Barycentric (Vec2f a, Vec2f b, Vec2f c, Vec2f p)
		{
			var ab = b - a;
			var ac = c - a;
			var pa = a - p;

			var r = Cross (new Vec3f {
				x = ab.x,
				y = ac.x,
				z = pa.x
			}, new Vec3f {
				x = ab.y,
				y = ac.y,
				z = pa.y
			});

			// triangle is degenerate, in this case return smth with negative coordinates
			// dont forget that r.z is integer. If it is zero then triangle ABC is degenerate 
			if ((int)r.z == 0)
				return new Vec3f { x = -1, y = 1, z = 1 };
			return new Vec3f { x = 1 - (r.x + r.y) / r.z, y = r.x / r.z, z = r.y / r.z };
		}

		// lesson 3
		public static void Triangle (Image image, Vec3f [] coordinates, Color color, float [] zbuffer)
		{
			Vec3f t0 = coordinates [0];
			Vec3f t1 = coordinates [1];
			Vec3f t2 = coordinates [2];

			var pMax = new Vec2i {
				x = (int)Math.Max (0, Math.Max (t0.x, Math.Max (t1.x, t2.x))),
				y = (int)Math.Max (0, Math.Max (t0.y, Math.Max (t1.y, t2.y)))
			};
			var pMin = new Vec2i {
				x = (int)Math.Min (image.Width, Math.Min (t0.x, Math.Min (t1.x, t2.x))),
				y = (int)Math.Min (image.Height, Math.Min (t0.y, Math.Min (t1.y, t2.y)))
			};

			for (int x = pMin.x; x <= pMax.x; x++) {
				for (int y = pMin.y; y <= pMax.y; y++) {
					var pixelCenter = new Vec2f {
						x = x + 0.5f,
						y = y + 0.5f
					};
					var bc = Barycentric (Project2D (t0), Project2D (t1), Project2D (t2), pixelCenter);
					if (bc.x < 0 || bc.y < 0 || bc.z < 0)
						continue;

					var z = t0.z * bc.x + t1.z * bc.y + t2.z * bc.z;
					var idx = x + y * image.Width;
					if (zbuffer [idx] < z) {
						zbuffer [idx] = z;
						image [x, y] = color;
					}
				}
			}
		}

		// lesson3
		public static float [] InitZBuffer (Image image)
		{
			var zbuffer = new float [image.Width * image.Height];
			for (int idx = 0; idx < zbuffer.Length; idx++)
				zbuffer [idx] = float.NegativeInfinity;
			return zbuffer;
		}

		// lesson3
		public static void Triangle (Image image, Vec3f [] coordinates, Image texture, Vec2f [] uv, float intensivity, float [] zbuffer)
		{
			Vec3f t0 = coordinates [0];
			Vec3f t1 = coordinates [1];
			Vec3f t2 = coordinates [2];

			var pMax = new Vec2i {
				x = (int)Math.Max (0, Math.Max (t0.x, Math.Max (t1.x, t2.x))),
				y = (int)Math.Max (0, Math.Max (t0.y, Math.Max (t1.y, t2.y)))
			};
			var pMin = new Vec2i {
				x = (int)Math.Min (image.Width, Math.Min (t0.x, Math.Min (t1.x, t2.x))),
				y = (int)Math.Min (image.Height, Math.Min (t0.y, Math.Min (t1.y, t2.y)))
			};

			for (int x = pMin.x; x <= pMax.x; x++) {
				for (int y = pMin.y; y <= pMax.y; y++) {
					var pixelCenter = new Vec2f {
						x = x + 0.5f,
						y = y + 0.5f
					};
					var bc = Barycentric (Project2D(t0), Project2D(t1), Project2D(t2), pixelCenter);
					if (bc.x < 0 || bc.y < 0 || bc.z < 0)
						continue;

					var z = t0.z * bc.x + t1.z * bc.y + t2.z * bc.z;
					var u = (int)(uv [0].x * bc.x + uv [1].x * bc.y + uv [2].x * bc.z);
					var v = (int)(uv [0].y * bc.x + uv [1].y * bc.y + uv [2].y * bc.z);
					var idx = x + y * image.Width;
					if (zbuffer [idx] < z) {
						zbuffer [idx] = z;

						var color = texture [u, v];
						image [x, y] = color * intensivity;
					}
				}
			}
		}

		public static void Box (Vec4f [] pts, out Vec2i pMin, out Vec2i pMax)
		{
			pMax = new Vec2i { x = int.MinValue, y = int.MinValue };
			pMin = new Vec2i { x = int.MaxValue, y = int.MaxValue };

			for (int i = 0; i < 3; i++) {
				pMax.x = (int)Math.Max (pMax.x, pts [i].x / pts [i].h);
				pMax.y = (int)Math.Max (pMax.y, pts [i].y / pts [i].h);
				pMin.x = (int)Math.Min (pMin.x, pts [i].x / pts [i].h);
				pMin.y = (int)Math.Min (pMin.y, pts [i].y / pts [i].h);
			}
		}

		public static void Triangle (Image image, Vec4f [] pts, IShader shader, float [] zbuffer)
		{
			Vec2i pMin, pMax;
			Box (pts, out pMin, out pMax);

			Color color;
			for (int x = pMin.x; x <= pMax.x; x++) {
				for (int y = pMin.y; y <= pMax.y; y++) {
					var pixelCenter = new Vec2f { x = x + 0.5f, y = y + 0.5f };
					var bc = Barycentric (Project2D (pts [0] / pts [0].h),
					                      Project2D (pts [1] / pts [1].h),
					                      Project2D (pts [2] / pts [2].h),
					                      pixelCenter);

					var z = pts [0].z * bc.x + pts [1].z * bc.y + pts [2].z * bc.z; // z [0..255]
					var w = pts [0].h * bc.x + pts [1].h * bc.y + pts [2].h * bc.z;
					var frag_depth = z / w;

					var idx = x + y * image.Width;
					if (bc.x < 0 || bc.y < 0 || bc.z < 0 || zbuffer[idx] > frag_depth)
						continue;


					var discard = shader.Fragment (new Vec3f { x = x, y = y, z = frag_depth }, bc, out color);
					if (!discard) {
						zbuffer [idx] = frag_depth;
						image [x, y] = color;
					}
				}
			}
		}

		// lesson4
		public static float DegToRad (float degAngle)
		{
			return degAngle * (float)Math.PI / 180;
		}

		// lesson4
		public static Matrix4 Viewport (int x, int y, int w, int h)
		{
			var depth = 255f;

			var m = Matrix4.Identity ();
			m [0, 3] = x + w / 2f;
			m [1, 3] = y + h / 2f;
			m [2, 3] = depth / 2f;

			m [0, 0] = w / 2f;
			m [1, 1] = h / 2f;
			m [2, 2] = depth / 2f;
			return m;
		}

		// lesson4
		public static Matrix4 Projection (float coeff)
		{
			var projection = Matrix4.Identity ();
			projection [3, 2] = coeff;
			return projection;
		}

		// lesson4
		public static Matrix4 LookAt (Vec3f eye, Vec3f center, Vec3f up)
		{
			var z = (eye - center).Normalize ();
			var x = Cross (up, z).Normalize ();
			var y = Cross (z, x).Normalize ();

			var Minv = Matrix4.Identity ();
			Minv [0, 0] = x.x; Minv [0, 1] = x.y; Minv [0, 2] = x.z;
			Minv [1, 0] = y.x; Minv [1, 1] = y.y; Minv [1, 2] = y.z;
			Minv [2, 0] = z.x; Minv [2, 1] = z.y; Minv [2, 2] = z.z;

			var Tr = Matrix4.Identity ();
			Tr [0, 3] = -center.x;
			Tr [1, 3] = -center.y;
			Tr [2, 3] = -center.y;

			return Minv * Tr;
		}

		public static Vec3f RandPointOnUnitSphere ()
		{
			var rnd = new Random ();
			var u = rnd.NextDouble ();
			var v = rnd.NextDouble ();
			var theta = 2 * Math.PI * u;
			var phi = Math.Acos(2 * v - 1);

			return new Vec3f {
				x = (float)(Math.Sin (phi) * Math.Cos (theta)),
				y = (float)(Math.Sin (phi) * Math.Sin (theta)),
				z = (float)Math.Cos (phi)
			};
		}

		public static float MaxElevationAngle (float [] zbuffer, Vec2f p, Vec2f dir, int width, int height)
		{
			float maxangle = 0;
			for (float t = 0; t < 1000; t += 1) {
				Vec2f cur = p + dir * t;
				if (cur.x >= width || cur.y >= height || cur.x < 0 || cur.y < 0)
					return maxangle;

				var distance = (p - cur).Norm ();
				if (distance < 1)
					continue;

				float elevation = zbuffer [(int)cur.x + (int)cur.y * width] - zbuffer [(int)p.x + (int)p.y * width];
				maxangle = (float)Math.Max (maxangle, Math.Atan (elevation / distance));
			}

			return maxangle;
		}
	}
}
