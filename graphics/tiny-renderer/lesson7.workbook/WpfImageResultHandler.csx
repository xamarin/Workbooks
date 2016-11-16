using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using XIR = Xamarin.Interactive.Representations;

static PixelFormat ConvertFormat (Format format)
{
	switch (format) {

		case Format.BGRA:
			return PixelFormats.Bgra32;
		case Format.GRAYSCALE:
			return PixelFormats.Gray8;
		case Format.BGR:
		default:
			return PixelFormats.Bgr24;
	}
}

InteractiveAgent.RepresentationManager.AddProvider<Image> (img => {
	var source = BitmapSource.Create (img.Width, img.Height, 96, 96, ConvertFormat (img.Format), null, img.buffer, img.BytesPerRow);
	var encoder = new PngBitmapEncoder ();
	var outputFrame = BitmapFrame.Create (source);
	encoder.Frames.Add (outputFrame);

	using (var memory = new MemoryStream ()) {
		encoder.Save (memory);
		return XIR.Image.FromPng (memory.GetBuffer (), img.Width, img.Height);
	}
});
