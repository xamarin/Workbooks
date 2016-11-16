int width = 800;
int height = 800;
var headModel = Model.FromFile ("obj/african_head.obj");

static void Swap<T>(ref T x, ref T y)
{
	 T t = y;
	 y = x;
	 x = t;
}

static void Swap<T>(T[] arr, int x, int y)
{
	 T t = arr[y];
	 arr[y] = arr[x];
	 arr[x] = t;
}

Image Line (Image image, int x0, int y0, int x1, int y1, Color color)
{
	bool steep = false;
	// if the line is steep, we transpose the image
	if (Math.Abs (x0-x1) < Math.Abs (y0-y1)) {
		Swap (ref x0, ref y0);
		Swap (ref x1, ref y1);
		steep = true;
	}
	if (x0 > x1) { // make it left to right
		Swap (ref x0, ref x1);
		Swap (ref y0, ref y1);
	}
	// (x0, y0) == (x1, y1)
	if(x0 == x1) {
		image [x0, y0] = color;
	} else {
		for (int x = x0; x <= x1; x++) {
			double t = (x-x0) / (double)(x1-x0);
			int y = (int)Math.Round(y0*(1-t) + y1*t);
			if (steep)
				image [y, x] = color;
			else
				image [x, y] = color; 
		}
	}
	return image;
}