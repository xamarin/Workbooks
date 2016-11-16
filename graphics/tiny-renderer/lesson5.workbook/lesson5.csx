using static Geometry;

var eye = new Vec3f { x = 1, y = 1, z = 3 };
var center = new Vec3f { x = 0, y = 0, z = 0 };
var up = new Vec3f { x = 0, y = 1, z = 0 };


Matrix4 LookAt (Vec3f eye, Vec3f center, Vec3f up)
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