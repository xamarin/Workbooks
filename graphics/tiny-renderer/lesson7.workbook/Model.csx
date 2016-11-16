using System.Collections.Generic;
using System.Linq;

struct Face
{
	public int [] Vertices;
	public int [] Textures;
	public int [] Normals;
}

class Model
{
	public List<Vec3f> Vertices { get; } = new List<Vec3f> ();
	public List<Face> Faces { get; } = new List<Face> ();
	public List<Vec3f> Textures { get; } = new List<Vec3f> ();
	public List<Vec3f> Normals { get; } = new List<Vec3f> ();

	public static Model FromFile (string path)
	{
		var model = new Model ();

		string line;
		using (var reader = new System.IO.StreamReader (path)) {
			while ((line = reader.ReadLine ()) != null)
				model.ParseLine (line);
		}

		Console.WriteLine ($"v#{model.Vertices.Count} f#{model.Faces.Count}");
		return model;
	}

	public static Model FromText (string text)
	{
		var model = new Model ();

		var lines = text.Split (new char [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var line in lines)
			model.ParseLine (line);

		Console.WriteLine ($"v#{model.Vertices.Count} f#{model.Faces.Count}");
		return model;
	}

	void ParseLine (string line)
	{
		Func<string [], Vec3f> parseV3f = strItems => new Vec3f {
			x = float.Parse (strItems [1]),
			y = float.Parse (strItems [2]),
			z = float.Parse (strItems [3])
		};

		var items = line.Split (new char [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (line.StartsWith ("v ", StringComparison.InvariantCulture)) {
			Vertices.Add (parseV3f (items));
		} else if (line.StartsWith ("vt ", StringComparison.InvariantCulture)) {
			Textures.Add (parseV3f (items));
		} else if (line.StartsWith ("vn ", StringComparison.InvariantCulture)) {
			Normals.Add (parseV3f (items));
		} else if (line.StartsWith ("f ", StringComparison.InvariantCulture)) {
			var indexes = items.Skip (1)
							   .SelectMany (s => s.Split (new char [] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries))
							   .Select (s => int.Parse (s) - 1) // in wavefront obj all indices start at 1, not zero
							   .ToArray ();
			Faces.Add (new Face {
				Vertices = indexes.Where ((v, index) => index % 3 == 0).ToArray (),
				Textures = indexes.Where ((v, index) => index % 3 == 1).ToArray (),
				Normals = indexes.Where ((v, index) => index % 3 == 2).ToArray ()
			});
		}
	}

	public Vec3f Normal (Face face, int nthvert)
	{
		int idx = face.Normals [nthvert];
		return Normals [idx];
	}

	public Vec3f Vertex (Face face, int nthvert)
	{
		int idx = face.Vertices [nthvert];
		return Vertices [idx];
	}

	public Vec3f GetUV (Face face, int nthvert)
	{
		int idx = face.Textures [nthvert];
		return Textures [idx];
	}
}
