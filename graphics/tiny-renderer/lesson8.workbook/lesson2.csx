using static Geometry;

var light_dir = new Vec3f { x = 0, y = 0, z = -1 };

var world = new Vec3f [3];
var screen = new Vec3f [3];

void Line (Image image, Vec2i p1, Vec2i p2, Color color)
{
	Line(image, p1.x, p1.y, p2.x, p2.y, color);
}

void Line (Image image, Vec3f p1, Vec3f p2, Color color)
{
	Line(image, (int)p1.x, (int)p1.y, (int)p2.x, (int)p2.y, color);
}

void Triangle (Image image, Vec2i[] t, Color color)
{
	Line(image, t[0], t[1], color);
	Line(image, t[1], t[2], color);
	Line(image, t[2], t[0], color);
}

Vec3f Barycentric (Vec3f a, Vec3f b, Vec3f c, Vec2i p)
{
    var pixel = new Vec3f { x = p.x + 0.5f, y = p.y + 0.5f };

    var ab = b - a;
    var ac = c - a;
    var pa = a - pixel;

    var r = Cross (new Vec3f { x = ab.x, y = ac.x, z = pa.x },
                   new Vec3f { x = ab.y, y = ac.y, z = pa.y });

    // triangle is degenerate, in this case return smth with negative coordinates 
    if (Math.Abs (r.z) < 1)
        return new Vec3f { x = -1, y = 1, z = 1 };

    return new Vec3f { x = 1 - r.x / r.z - r.y / r.z, y = r.x / r.z, z = r.y / r.z };
}

void Triangle (Image image, Vec3f [] coordinates, Color color)
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