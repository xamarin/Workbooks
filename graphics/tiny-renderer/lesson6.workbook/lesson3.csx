using static Geometry;

var headTexture = Image.Load ("obj/african_head_diffuse.tga");
headTexture.VerticalFlip ();

float [] InitZBuffer (Image image)
{
	var zbuffer = new float [image.Width * image.Height];
	for (int idx = 0; idx < zbuffer.Length; idx++)
		zbuffer [idx] = float.NegativeInfinity;
	return zbuffer;
}

Vec3f Barycentric (Vec2f a, Vec2f b, Vec2f c, Vec2f p)
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

void Triangle (Image image, Vec3f [] coordinates, Image texture, Vec2f [] uv, float intensivity, float [] zbuffer)
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
