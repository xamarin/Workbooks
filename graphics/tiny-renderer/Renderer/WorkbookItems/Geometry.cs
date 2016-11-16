using System;

namespace Renderer
{
	struct Vec2f
	{
		public float x;
		public float y;

		public float this [int i] {
			get {
				if (i == 0) return x;
				if (i == 1) return y;
				throw new InvalidOperationException ();
			}
			set {
				if (i == 0) x = value;
				else if (i == 1) y = value;
				else throw new InvalidOperationException ();
			}
		}

		public Vec2f Normalize ()
		{
			return this / Norm ();
		}

		public float Norm ()
		{
			return (float)Math.Sqrt (x * x + y * y);
		}

		public static Vec2f operator / (Vec2f v, float num)
		{
			v.x /= num;
			v.y /= num;

			return v;
		}

		public static Vec2f operator * (Vec2f v, float num)
		{
			v.x *= num;
			v.y *= num;

			return v;
		}

		public static Vec2f operator - (Vec2f a, Vec2f b)
		{
			return new Vec2f { x = a.x - b.x, y = a.y - b.y };
		}

		public static Vec2f operator + (Vec2f a, Vec2f b)
		{
			return new Vec2f { x = a.x + b.x, y = a.y + b.y };
		}
	}

	public struct Vec3f
	{
		public float x;
		public float y;
		public float z;

		public float this [int i] {
			get {
				switch (i) {
				case 0: return x;
				case 1: return y;
				case 2: return z;
				default: throw new InvalidOperationException ();
				}
			}
			set {
				switch (i) {
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				default: throw new InvalidOperationException ();
				}
			}
		}

		public Vec3f Normalize ()
		{
			return this / Norm ();
		}

		public float Norm ()
		{
			return (float)Math.Sqrt (x * x + y * y + z * z);
		}

		public static Vec3f operator - (Vec3f a, Vec3f b)
		{
			return new Vec3f { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
		}

		public static Vec3f operator / (Vec3f v, float num)
		{
			v.x /= num;
			v.y /= num;
			v.z /= num;

			return v;
		}

		public static Vec3f operator * (Vec3f v, float num)
		{
			v.x *= num;
			v.y *= num;
			v.z *= num;

			return v;
		}
	}

	struct Vec4f
	{
		public float x;
		public float y;
		public float z;
		public float h;

		public float this [int i] {
			get {
				switch (i) {
					case 0: return x;
					case 1: return y;
					case 2: return z;
					case 3: return h;
					default: throw new InvalidOperationException ();
				}
			}
			set {
				switch (i) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: h = value; break;
					default: throw new InvalidOperationException ();
				}
			}
		}

		public Vec4f Normalize ()
		{
			var len = Norm ();
			return this / len;
		}

		public float Norm ()
		{
			return (float)Math.Sqrt (x * x + y * y + z * z + h * h);
		}

		public static Vec4f operator - (Vec4f a, Vec4f b)
		{
			return new Vec4f { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z, h = a.h - b.h };
		}

		public static Vec4f operator / (Vec4f v, float num)
		{
			v.x /= num;
			v.y /= num;
			v.z /= num;
			v.h /= num;

			return v;
		}
	}

	struct Vec2i
	{
		public int x;
		public int y;

		public static Vec2i operator - (Vec2i a, Vec2i b)
		{
			return new Vec2i { x = a.x - b.x, y = a.y - b.y };
		}
	}

	struct Vec3i
	{
		public int x;
		public int y;
		public int z;

		public static Vec3i operator - (Vec3i a, Vec3i b)
		{
			return new Vec3i { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
		}
	}

	static class Geometry
	{
		public static Vec3f Cross (Vec3f l, Vec3f r)
		{
			return new Vec3f {
				x = l.y * r.z - l.z * r.y,
				y = l.z * r.x - l.x * r.z,
				z = l.x * r.y - l.y * r.x
			};
		}

		public static float Dot (Vec3f l, Vec3f r)
		{
			return l.x * r.x + l.y * r.y + l.z * r.z;
		}

		public static Vec4f Embed4D (Vec3f v, float fill = 1)
		{
			return new Vec4f { x = v.x, y = v.y, z = v.z, h = fill };
		}

		public static Vec2f Project2D (Vec3f v)
		{
			return new Vec2f { x = v.x, y = v.y };
		}

		public static Vec2f Project2D (Vec4f v)
		{
			return new Vec2f { x = v.x, y = v.y };
		}

		public static Vec3f Project3D (Vec4f v)
		{
			return new Vec3f { x = v.x, y = v.y, z = v.z };
		}
	}
}
