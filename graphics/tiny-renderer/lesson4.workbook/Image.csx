using System.IO;

enum Format
{
	GRAYSCALE = 1,
	BGR = 3,
	BGRA = 4
}

struct Color
{
	// the value stored as little endian:
	// ARGB -> BGRA
	// xRGB -> BGRx
	public readonly int value;
	public readonly Format format;

	public byte this [int offset] {
		get {
			if (offset > 3) // int has only 4 bytes (0, 1, 2, 3)
				throw new ArgumentOutOfRangeException ();
			return (byte)(value >> 8 * (3 - offset));
		}
	}

	public static Color Red = new Color (red: 255, green: 0, blue: 0, alpha: 255);
	public static Color Green = new Color (red: 0, green: 255, blue: 0, alpha: 255);
	public static Color Blue = new Color (red: 0, green: 0, blue: 255, alpha: 255);
	public static Color Black = new Color (red: 0, green: 0, blue: 0, alpha: 255);
	public static Color White = new Color (red: 255, green: 255, blue: 255, alpha: 255);
	public static Color Yellow = new Color (red: 225, green: 225, blue: 0, alpha: 255);

	public Color (byte red, byte green, byte blue, byte alpha)
		: this ((blue << 24) | (green << 16) | (red << 8) | alpha, Format.BGRA)
	{
	}

	public Color (byte red, byte green, byte blue)
		: this ((blue << 24) | (green << 16) | (red << 8) | 0xFF, Format.BGR)
	{
	}

	public Color (byte value)
		: this (value, Format.GRAYSCALE)
	{
	}

	public Color (int value, Format format)
	{
		this.value = value;
		this.format = format;
	}

	public static Color operator * (Color color, float intensivity)
	{
		intensivity = Math.Max (0f, Math.Min (1f, intensivity));
		var ch0 = (byte)(color [0] * intensivity);
		var ch1 = (byte)(color [1] * intensivity);
		var ch2 = (byte)(color [2] * intensivity);
		var ch3 = color [3];
		return new Color (ch0 << 24 | ch1 << 16 | ch2 << 8 | ch3, color.format);
	}
}

class Image
{
	internal byte [] buffer;

	public int Width { get; }
	public int Height { get; }
	public Format Format { get; }

	public int BytesPerRow {
		get {
			return Width * (int)Format;
		}
	}

	public Image (int width, int height, Format format)
	{
		Width = width;
		Height = height;
		Format = format;

		buffer = new byte [height * BytesPerRow];
	}

	public void VerticalFlip ()
	{
		var bpp = (int)Format;
		int bytesPerLine = Width * bpp;

		var half = Height >> 1;
		for (int l = 0; l < half; l++) {
			var l1 = l * bytesPerLine;
			var l2 = (Height - 1 - l) * bytesPerLine;

			for (int i = 0; i < bytesPerLine; i++) {
				byte pixel = buffer [l1 + i];
				buffer [l1 + i] = buffer [l2 + i];
				buffer [l2 + i] = pixel;
			}
		}
	}

	public void Clear ()
	{
		for (int i = 0; i < buffer.Length; i++)
			buffer [i] = 0;
	}

	public Color this [int x, int y] {
		get {
			if (x < 0 || x >= Width) throw new ArgumentException ("x");
			if (y < 0 || y >= Height) throw new ArgumentException ("y");

			var offset = GetOffset (x, y);
			var len = (int)Format;
			int value = 0;
			for (var ch = 0; ch < 4; ch++)
				value = (value << 8) | (ch < len ? buffer [offset++] : 0xFF);

			return new Color (value, Format);
		}
		set {
			if (x < 0 || x >= Width) return; //throw new ArgumentException ($"{nameof(x)}={x} {nameof(Width)}={Width}");
			if (y < 0 || y >= Height) return; // throw new ArgumentException ($"{nameof(y)}={y} {nameof(Height)}={Height}");

			var offset = GetOffset (x, y);
			var v = value.value;
			var len = (int)Format;
			for (int ch = 0; ch < len; ch++)                   // 0123
				buffer [offset++] = (byte)(v >> (3 - ch) * 8); // BGRA
		}
	}

	int GetOffset (int x, int y)
	{
		return y * BytesPerRow + x * (int)Format;
	}

	public bool WriteToFile (string path, bool rle = true)
	{
		var bpp = (int)Format;
		using (var writer = new BinaryWriter (File.Create (path))) {
			var header = new TGAHeader {
				IdLength = 0, // The IDLength set to 0 indicates that there is no image identification field in the TGA file
				ColorMapType = 0, // a value of 0 indicates that no palette is included
				BitsPerPixel = (byte)(bpp * 8),
				Width = (short)Width,
				Height = (short)Height,
				DataTypeCode = DataTypeFor (bpp, rle),
				ImageDescriptor = (byte)(0x20 | (Format == Format.BGRA ? 8 : 0)) // top-left origin
			};
			WriteTo (writer, header);
			if (!rle)
				writer.Write (buffer);
			else
				UnloadRleData (writer);
		}
		return true;
	}

	public static Image Load (string path)
	{
		using (var reader = new BinaryReader (File.OpenRead (path))) {
			var header = ReadHeader (reader);

			var height = header.Height;
			var width = header.Width;
			var bytespp = header.BitsPerPixel >> 3;
			var format = (Format)bytespp;

			if (width <= 0 || height <= 0)
				throw new InvalidProgramException ($"bad image size: width={width} height={height}");
			if (format != Format.BGR && format != Format.BGRA && format != Format.GRAYSCALE)
				throw new InvalidProgramException ($"unknown format {format}");

			var img = new Image (width, height, format);

			switch (header.DataTypeCode) {
			case DataType.UncompressedTrueColorImage:
			case DataType.UncompressedBlackAndWhiteImage:
				reader.Read (img.buffer, 0, img.buffer.Length);
				break;
			case DataType.RleTrueColorImage:
			case DataType.RleBlackAndWhiteImage:
				img.LoadRleData (reader);
				break;
			default:
				throw new InvalidProgramException ($"unsupported image format {header.DataTypeCode}");
			}

			if ((header.ImageDescriptor & 0x20) == 0)
				img.VerticalFlip ();

			return img;
		}
	}

	static void WriteTo (BinaryWriter writer, TGAHeader header)
	{
		writer.Write (header.IdLength);
		writer.Write (header.ColorMapType);
		writer.Write ((byte)header.DataTypeCode);
		writer.Write (header.ColorMapOrigin);
		writer.Write (header.ColorMapLength);
		writer.Write (header.ColorMapDepth);
		writer.Write (header.OriginX);
		writer.Write (header.OriginY);
		writer.Write (header.Width);
		writer.Write (header.Height);
		writer.Write (header.BitsPerPixel);
		writer.Write (header.ImageDescriptor);
	}

	static TGAHeader ReadHeader (BinaryReader reader)
	{
		var header = new TGAHeader {
			IdLength = reader.ReadByte (),
			ColorMapType = reader.ReadByte (),
			DataTypeCode = (DataType)reader.ReadByte (),
			ColorMapOrigin = reader.ReadInt16 (),
			ColorMapLength = reader.ReadInt16 (),
			ColorMapDepth = reader.ReadByte (),
			OriginX = reader.ReadInt16 (),
			OriginY = reader.ReadInt16 (),
			Width = reader.ReadInt16 (),
			Height = reader.ReadInt16 (),
			BitsPerPixel = reader.ReadByte (),
			ImageDescriptor = reader.ReadByte ()
		};
		return header;
	}

	bool UnloadRleData (BinaryWriter writer)
	{
		const int max_chunk_length = 128;
		int npixels = Width * Height;
		int curpix = 0;
		var bpp = (int)Format;

		while (curpix < npixels) {
			int chunkstart = curpix * bpp;
			int curbyte = curpix * bpp;
			int run_length = 1;
			bool literal = true;
			while (curpix + run_length < npixels && run_length < max_chunk_length && curpix + run_length < curpix + Width) {
				bool succ_eq = true;
				for (int t = 0; succ_eq && t < bpp; t++)
					succ_eq = (buffer [curbyte + t] == buffer [curbyte + t + bpp]);
				curbyte += bpp;
				if (1 == run_length)
					literal = !succ_eq;
				if (literal && succ_eq) {
					run_length--;
					break;
				}
				if (!literal && !succ_eq)
					break;
				run_length++;
			}
			curpix += run_length;

			writer.Write ((byte)(literal ? run_length - 1 : 128 + (run_length - 1)));
			writer.Write (buffer, chunkstart, literal ? run_length * bpp : bpp);
		}
		return true;
	}

	void LoadRleData (BinaryReader reader)
	{
		var pixelcount = Width * Height;
		var currentpixel = 0;
		var currentbyte = 0;

		var bytespp = (int)Format;
		var color = new byte [4];

		do {
			var chunkheader = reader.ReadByte ();
			if (chunkheader < 128) {
				chunkheader++;
				for (int i = 0; i < chunkheader; i++) {
					for (int t = 0; t < bytespp; t++)
						buffer [currentbyte++] = reader.ReadByte ();
					currentpixel++;
					if (currentpixel > pixelcount)
						throw new InvalidProgramException ("Too many pixels read");
				}
			} else {
				chunkheader -= 127;
				reader.Read (color, 0, bytespp);
				for (int i = 0; i < chunkheader; i++) {
					for (int t = 0; t < bytespp; t++)
						buffer [currentbyte++] = color [t];
					currentpixel++;
					if (currentpixel > pixelcount)
						throw new InvalidProgramException ("Too many pixels read");
				}
			}
		} while (currentpixel < pixelcount);
	}

	static DataType DataTypeFor (int bpp, bool rle)
	{
		var format = (Format)bpp;
		if (format == Format.GRAYSCALE)
			return rle ? DataType.RleBlackAndWhiteImage : DataType.UncompressedBlackAndWhiteImage;
		return rle ? DataType.RleTrueColorImage : DataType.UncompressedTrueColorImage;
	}
}

struct TGAHeader
{
	public byte IdLength;
	public byte ColorMapType;
	public DataType DataTypeCode;

	// field #4. Color map specification
	public short ColorMapOrigin; // index of first color map entry that is included in the file
	public short ColorMapLength; // number of entries of the color map that are included in the file
	public byte ColorMapDepth;   // number of bits per pixel

	// field #5. Image specification
	public short OriginX; // absolute coordinate of lower-left corner for displays where origin is at the lower left
	public short OriginY; // as for X-origin
	public short Width;   // width in pixels
	public short Height;  // height in pixels
	public byte BitsPerPixel;     // pixel depth
	public byte ImageDescriptor;  // bits 3-0 give the alpha channel depth, bits 5-4 give direction
}

public enum DataType : byte
{
	NoImageData = 0, // no image data is present
	UncompressedColorMappedImage = 1,
	UncompressedTrueColorImage = 2,
	UncompressedBlackAndWhiteImage = 3,
	RleColorMappedImage = 9, // run-length encoded color-mapped image
	RleTrueColorImage = 10, // run-length encoded true-color image
	RleBlackAndWhiteImage = 11 // run-length encoded black-and-white (grayscale) image
}
