using System;
using static Renderer.Geometry;
using static Renderer.MatrixHelpers;
using static Renderer.ShaderUtils;

namespace Renderer
{
	// lesson6
	interface IShader
	{
		Vec4f Vertex (Face face, int nthvert);
		bool Fragment (Vec3f fragment, Vec3f bar, out Color color);
	};

	// lesson6
	class GouraudShader : IShader
	{
		readonly Model model;
		readonly Vec3f lightDir;

		// written by vertex shader, read by fragment shader
		protected Vec3f varyingIntensity = new Vec3f ();

		readonly Matrix4 transformation;

		public GouraudShader (Model model, Matrix4 viewPort, Matrix4 projection, Matrix4 modelView, Vec3f lightDir)
		{
			this.model = model;
			transformation = viewPort * projection * modelView;
			this.lightDir = lightDir.Normalize ();
		}

		public virtual Vec4f Vertex (Face face, int nthvert)
		{
			var n = model.Normal (face, nthvert).Normalize ();
			varyingIntensity [nthvert] = Math.Max (0, Dot (n, lightDir)); // get diffuse lighting intensity

			return TransformFace (model, face, nthvert, transformation);
		}

		public virtual bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var intensity = Dot (varyingIntensity, bar);   // interpolate intensity for the current pixel
			color = Color.White * intensity; // well duh
			return false;
		}
	}

	class GouraudShader6 : GouraudShader
	{
		public GouraudShader6 (Model model, Matrix4 viewPort, Matrix4 projection, Matrix4 modelView, Vec3f lightDir)
			: base (model, viewPort, projection, modelView, lightDir)
		{
		}

		public override bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var intensity = Dot (varyingIntensity, bar);
			if (intensity > 0.85f) intensity = 1;
			else if (intensity > 0.60f) intensity = 0.80f;
			else if (intensity > 0.45f) intensity = 0.60f;
			else if (intensity > 0.30f) intensity = 0.45f;
			else if (intensity > 0.15f) intensity = 0.30f;
			else intensity = 0;
			color = new Color (255, 155, 0) * intensity;
			return false;
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

	class NormalMapShader : IShader
	{
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();

		readonly Model model;
		readonly Vec3f lightDir;
		readonly Matrix4 uniformM;
		readonly Matrix4 uniformMIT;
		readonly Matrix4 transformation;

		readonly Image texture;
		readonly Image normalMap;

		public NormalMapShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Vec3f lightDir, Image texture, Image normalMap)
		{
			this.model = model;
			this.lightDir = lightDir.Normalize ();
			this.texture = texture;
			this.normalMap = normalMap;

			uniformM = projection * modelView;
			uniformMIT = TransposeInverse (uniformM);
			transformation = viewport * uniformM;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var uv = CalcUV (varyingU, varyingV, bar);
			var n = Transform (uniformMIT, Normal (normalMap, uv)).Normalize ();
			var l = Transform (uniformM, lightDir).Normalize ();

			var intensity = Math.Max (0f, Dot (n, l));
			color = GetColor (texture, uv) * intensity;
			return false;
		}
	}

	class SpecularShader : IShader
	{
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();

		readonly Model model;
		readonly Vec3f lightDir;
		readonly Matrix4 uniformM;
		readonly Matrix4 uniformMIT;
		readonly Matrix4 transformation;

		readonly Image texture;
		readonly Image normalMap;
		readonly Image specularMap;

		public SpecularShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Vec3f lightDir, Image texture, Image normalMap, Image specularMap)
		{
			this.model = model;
			this.lightDir = lightDir.Normalize ();
			this.texture = texture;
			this.normalMap = normalMap;
			this.specularMap = specularMap;

			uniformM = projection * modelView;
			uniformMIT = TransposeInverse (uniformM);
			transformation = viewport * uniformM;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var uv = CalcUV (varyingU, varyingV, bar);
			var n = Transform (uniformMIT, Normal (normalMap, uv)).Normalize ();
			var l = Transform (uniformM, lightDir).Normalize ();

			var r = (n * (2 * Dot (n, l)) - l).Normalize ();

			var diff = Math.Max (0f, Dot (n, l));
			var specular = Math.Pow (Math.Max (0f, r.z), Specular (specularMap, uv) + 15);

			color = GetColor (texture, uv);

			int v = 0;
			for (int i = 0; i < 4; i++)
				v = (v << 8) | (byte)Math.Min (255, (int)(5 + color [i] * (diff + 1.3f * specular)));
			color = new Color (v, color.format);

			return false;
		}
	}

	class TangentShader : IShader
	{
		// triangle uv coordinates, written by the vertex shader, read by the fragment shader
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();

		// normal per vertex to be interpolated by FS
		Matrix3 varyingNrm = new Matrix3 ();

		readonly Model model;
		readonly Vec3f lightDir;
		readonly Matrix4 uniformM;
		readonly Matrix4 uniformMIT;

		// do not need for starting poing
		readonly Matrix4 transformation;
		readonly Vec3f[] ndcTri = new Vec3f[3];     // triangle in normalized device coordinates

		readonly Image texture;
		// do not need for starting poing
		readonly Image normalMap;

		// use this commit for 6bis lesson:
		// https://github.com/xamarin/private-samples/commit/ddad38f7787d5e9c065afc91547730ad38e51fd1#diff-8603c43660c04f4d430053b29c80ed15R253
		public TangentShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Vec3f lightDir, Image texture, Image normalMap)
		{
			this.model = model;
			this.lightDir = lightDir.Normalize ();
			this.texture = texture;

			// do not need for starting poing
			 this.normalMap = normalMap;
			// this.specularMap = specularMap;

			uniformM = projection * modelView;
			uniformMIT = TransposeInverse (uniformM);
			transformation = viewport * uniformM;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);

			var normal = Project3D (Mult (uniformMIT, Embed4D (model.Normal (face, nthvert))));
			varyingNrm.SetColumn (nthvert, normal);

			var glVertex = TransformFace (model, face, nthvert, transformation);
			ndcTri[nthvert] = Project3D (glVertex / glVertex.h);

			return glVertex;
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var bn = Mult(varyingNrm, bar).Normalize ();
			var uv = CalcUV (varyingU, varyingV, bar);

			var A = new Matrix3 ();
			A.SetRow (0, ndcTri [1] - ndcTri [0]);
			A.SetRow (1, ndcTri [2] - ndcTri [0]);
			A.SetRow (2, bn);

			var AI = Inverse (A);
			var i = Mult (AI, new Vec3f { x = varyingU.y - varyingU.x, y = varyingU.z - varyingU.x });
			var j = Mult (AI, new Vec3f { x = varyingV.y - varyingV.x, y = varyingV.z - varyingV.x });

			var B = new Matrix3 ();
			B.SetColumn (0, i.Normalize ());
			B.SetColumn (1, j.Normalize ());
			B.SetColumn (2, bn);

			var n = Mult (B, Normal (normalMap, uv)).Normalize ();

			var l = Transform (uniformM, lightDir).Normalize ();
			var diff = Math.Max (0f, Dot (n, l));

			color = GetColor (texture, uv) * diff;
			return false;
		}
	}

	class DepthShader : IShader
	{
		const float depth = 255f;

		Matrix3 varyingTri;

		readonly Model model;
		readonly Matrix4 transformation;

		public DepthShader (Model model, Matrix4 transformation)
		{
			this.model = model;
			this.transformation = transformation;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			var glVertex = TransformFace (model, face, nthvert, transformation);
			varyingTri.SetColumn (nthvert, Project3D (glVertex / glVertex.h));
			return glVertex;
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var p = Mult (varyingTri, bar);
			color = Color.White * (p.z / depth);
			return false;
		}
	};

	class ShadowShader : IShader
	{
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();
		Matrix3 varyingTri;

		readonly Model model;
		readonly Vec3f lightDir;
		readonly Matrix4 uniformM;
		readonly Matrix4 uniformMIT;
		readonly Matrix4 uniformShadow; // transform framebuffer screen coordinates to shadowbuffer screen coordinates
		readonly Matrix4 transformation;

		readonly Image texture;
		readonly Image normalMap;
		readonly Image specularMap;

		readonly float[] shadowbuffer;
		readonly int width;

		public ShadowShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Matrix4 uniformShadow, Vec3f lightDir, Image texture, Image normalMap, Image specularMap, float[] shadowbuffer, int width)
		{
			this.model = model;
			this.lightDir = lightDir.Normalize ();
			this.texture = texture;
			this.normalMap = normalMap;
			this.specularMap = specularMap;
			this.shadowbuffer = shadowbuffer;
			this.width = width;

			uniformM = projection * modelView;
			uniformMIT = TransposeInverse (uniformM);
			transformation = viewport * uniformM;
			this.uniformShadow = uniformShadow;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);

			var glVertex = TransformFace (model, face, nthvert, transformation);
			varyingTri.SetColumn (nthvert, Project3D (glVertex / glVertex.h));

			return glVertex; 
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var sb_p = Mult (uniformShadow, Embed4D (Mult (varyingTri, bar))); // corresponding point in the shadow buffer
			sb_p = sb_p / sb_p.h;
			int idx = (int)sb_p.x + (int)sb_p.y * width; // index in the shadowbuffer array
			// z-fighting: sb_p.z + 3.5f
			float shadow = 0.3f + 0.7f * (shadowbuffer [idx] < (sb_p.z + 3.5f) ? 1f : 0f);

			var uv = CalcUV (varyingU, varyingV, bar);
			var n = Transform (uniformMIT, Normal (normalMap, uv)).Normalize ();
			var l = Transform (uniformM, lightDir).Normalize ();
			var r = (n * (2 * Dot (n, l)) - l).Normalize ();
			var diff = Math.Max (0f, Dot (n, l));

			var specular = Math.Pow (Math.Max (0f, r.z), Specular (specularMap, uv) + 15);
			color = GetColor (texture, uv);

			int v = 0;
			for (int i = 0; i < 4; i++)
				v = (v <<= 8) | (byte)Math.Min (255, (int)(5 + color [i] * shadow * (diff + 1.3f * specular)));
			color = new Color (v, color.format);

			return false;
		}
	}

	#region Lesson9

	class ZShader : IShader
	{
		const float depth = 255f;

		readonly Model model;
		readonly Matrix4 transformation;

		public ZShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView)
		{
			this.model = model;
			transformation = viewport * projection * modelView;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			color = Color.White * (fragment.z / depth);
			return false;
		}
	}

	class OcclusionShader : IShader
	{
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();

		readonly Model model;
		readonly Matrix4 transformation;
		readonly Image occlusion;
		readonly float [] shadowbuffer;
		readonly int width;

		public OcclusionShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Image occlusion, float [] shadowbuffer, int width)
		{
			this.model = model;
			this.occlusion = occlusion;
			this.shadowbuffer = shadowbuffer;
			this.width = width;

			transformation = viewport * projection * modelView;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var uvf = CalcUV (varyingU, varyingV, bar);
			var uvi = CalcXY (occlusion, uvf);

			var index = (int)(fragment.x + fragment.y * width);
			if (Math.Abs(shadowbuffer [index] - (byte)fragment.z) <= 1e-2)
				occlusion [uvi.x, uvi.y] = Color.White;

			color = Color.White;
			return false;
		}
	}

	class AOShader : IShader
	{
		Vec3f varyingU = new Vec3f ();
		Vec3f varyingV = new Vec3f ();

		readonly Model model;
		readonly Matrix4 transformation;
		readonly Image aoImage;

		public AOShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Image aoImage)
		{
			this.model = model;
			this.aoImage = aoImage;

			transformation = viewport * projection * modelView;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			var uvf = CalcUV (varyingU, varyingV, bar);
			var uvi = CalcXY (aoImage, uvf);

			var t = aoImage [uvi.x, uvi.y] [0];
			color = new Color (t, t, t);

			return false;
		}
	};

	// Name it ZShader in wb (redefine class)
	class ZShader2 : IShader
	{
		readonly Model model;
		readonly Matrix4 transformation;

		public ZShader2 (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView)
		{
			this.model = model;
			transformation = viewport * projection * modelView;
		}

		public Vec4f Vertex (Face face, int nthvert)
		{
			return TransformFace (model, face, nthvert, transformation);
		}

		public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
		{
			color = Color.Black;
			return false;
		}
	};

	#endregion

	static class ShaderUtils
	{
		// lesson6
		public static Vec4f TransformFace (Model model, Face face, int nthvert, Matrix4 t)
		{
			var v = model.Vertex (face, nthvert); // read the vertex from model
			var glVertex = Embed4D (v);
			return Mult (t, glVertex); // transform it to screen coordinates
		}

		// lesson6
		public static void UpdateVarayingUV (Model model, Face face, int nthvert, ref Vec3f varyingU, ref Vec3f varyingV)
		{
			var vt = model.GetUV (face, nthvert);
			varyingU [nthvert] = vt.x;
			varyingV [nthvert] = vt.y;
		}

		// lesson6
		public static Vec2f CalcUV (Vec3f varU, Vec3f varV, Vec3f bar)
		{
			return new Vec2f {
				x = Dot (varU, bar),
				y = Dot (varV, bar)
			};
		}

		// lesson6
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

		// lesson6
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

		// lesson6
		public static float Specular (Image specularMap, Vec2f uvf)
		{
			var uvi = CalcXY (specularMap, uvf);
			var color = specularMap [uvi.x, uvi.y];
			return color[0];
		}

		// lesson6
		public static Vec2i CalcXY (Image texture, Vec2f uvf)
		{
			return new Vec2i {
				x = (int)(uvf.x * texture.Width),
				y = (int)(uvf.y * texture.Height)
			};
		}
	}
}
