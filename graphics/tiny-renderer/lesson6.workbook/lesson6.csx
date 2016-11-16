using static Geometry;
using static MatrixHelpers;
using static ShaderUtils;

var diabloModel = Model.FromFile ("obj/diablo3_pose.obj");
var diabloTexture = Image.Load ("obj/diablo3_pose_diffuse.tga");
var diabloNormalMap = Image.Load ("obj/diablo3_pose_nm.tga");
var diabloTangentMap = Image.Load ("obj/diablo3_pose_nm_tangent.tga");
var diabloSpecularMap = Image.Load ("obj/diablo3_pose_spec.tga");
diabloTexture.VerticalFlip ();
diabloNormalMap.VerticalFlip ();
diabloTangentMap.VerticalFlip ();
diabloSpecularMap.VerticalFlip ();

light_dir = new Vec3f { x = 1, y = 1, z = 1 };
eye = new Vec3f { x = 1, y = 1, z = 3 };
center = new Vec3f { x = 0, y = 0, z = 0 };
up = new Vec3f { x = 0, y = 1, z = 0 };

var modelView = LookAt(eye, center, up);
var viewPort = Viewport(width/8, height/8, width*3/4, height*3/4);
var projection = Projection(-1f/(eye-center).Norm());

interface IShader
{
	Vec4f Vertex (Face face, int nthvert);
	bool Fragment (Vec3f fragment, Vec3f bar, out Color color);
};

class RenderResult
{
	public Image Image { get; set; }
	public float [] ZBuffer { get; set; }
}

RenderResult Render (Model model, IShader shader)
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

void Box (Vec4f [] pts, out Vec2i pMin, out Vec2i pMax)
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

void Triangle (Image image, Vec4f [] pts, IShader shader, float [] zbuffer)
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


			var fragment = new Vec3f { x = x, y = y, z = frag_depth };
			var discard = shader.Fragment (fragment, bc, out color);
			if (!discard) {
				zbuffer [idx] = frag_depth;
				image [x, y] = color;
			}
		}
	}
}

static class ShaderUtils
{
    public static Vec4f TransformFace (Model model, Face face, int nthvert, Matrix4 t)
    {
        var v = model.Vertex (face, nthvert); // read the vertex from model
        var glVertex = Embed4D (v);
        return Mult (t, glVertex); // transform it to screen coordinates
    }

    public static void UpdateVarayingUV (Model model, Face face, int nthvert,
                                         ref Vec3f varyingU, ref Vec3f varyingV)
    {
        var vt = model.GetUV (face, nthvert);
        varyingU [nthvert] = vt.x;
        varyingV [nthvert] = vt.y;
    }

    public static Vec2f CalcUV (Vec3f varU, Vec3f varV, Vec3f bar)
    {
        return new Vec2f {
            x = Dot (varU, bar),
            y = Dot (varV, bar)
        };
    }

    public static Color GetColor (Image texture, Vec2f uvf)
    {
        var uvi = CalcXY(texture, uvf);
        return texture [uvi.x, uvi.y];
    }

    public static Vec3f Transform (Matrix4 t, Vec3f v)
    {
        var v4d = Mult (t, Embed4D (v));
        return Project3D (v4d);
    }

    public static Vec3f Normal (Image normalMap, Vec2f uvf)
    {
        // RGB values as xyz. But Color stores data as BGR (zyx)
        var c = GetColor (normalMap, uvf);

        return new Vec3f {
            x = (c [2] / 255f) * 2 - 1,
            y = (c [1] / 255f) * 2 - 1,
            z = (c [0] / 255f) * 2 - 1
        };
    }

    public static float Specular (Image specularMap, Vec2f uvf)
    {
        var uvi = CalcXY (specularMap, uvf);
        var color = specularMap [uvi.x, uvi.y];
        return color[0];
    }

    public static Vec2i CalcXY (Image texture, Vec2f uvf)
    {
        return new Vec2i {
            x = (int)(uvf.x * texture.Width),
            y = (int)(uvf.y * texture.Height)
        };
    }
}

class TextureShader : IShader
{
	readonly Model model;
	readonly Vec3f lightDir;
	readonly Matrix4 transformation;
	readonly Image texture;

	// written by vertex shader, read by fragment shader
	Vec3f varyingIntensity = new Vec3f ();
	Vec3f varyingU = new Vec3f ();
	Vec3f varyingV = new Vec3f ();

	public TextureShader (Model model, Matrix4 viewPort, Matrix4 projection, Matrix4 modelView, Vec3f lightDir, Image texture)
	{
		this.model = model;
		transformation = viewPort * projection * modelView;
		this.lightDir = lightDir.Normalize ();
		this.texture = texture;
	}

	public Vec4f Vertex (Face face, int nthvert)
	{
		UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);

		var n = model.Normal (face, nthvert).Normalize ();
		varyingIntensity [nthvert] = Math.Max (0, Dot (n, lightDir)); // get diffuse lighting intensity

		return TransformFace (model, face, nthvert, transformation);
	}

	public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
	{
		// interpolate intensity for the current pixel
		var intensity = Dot (varyingIntensity, bar);

		// interpolate uv for the current pixel
		var uvf = CalcUV (varyingU, varyingV, bar);

		color = GetColor (texture, uvf) * intensity;
		return false;
	}
}
