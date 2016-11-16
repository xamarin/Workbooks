Vec2f[] uv = new Vec2f [3];

Func<Image, Vec3f, Vec2f> uvMap = (texture, v) => new Vec2f {
	x = v.x * (texture.Width - 1),
	y = v.y * (texture.Height - 1)
};

Matrix4 Viewport (int x, int y, int w, int h)
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

Matrix4 Projection (float coeff)
{
	var projection = Matrix4.Identity ();
	projection [3, 2] = coeff;
	return projection;
}