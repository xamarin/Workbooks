using XIR = Xamarin.Interactive.Representations;

InteractiveAgent.RepresentationManager.AddProvider<Image> (img => {
    XIR.ImageFormat format;

    switch (img.Format) {
    case Format.BGRA:
        format = XIR.ImageFormat.Bgra32;
        break;
    case Format.BGR:
        format = XIR.ImageFormat.Bgr24;
        break;
    default:
        return null;
    }

    return new XIR.Image (format, img.buffer, img.Width, img.Height);
});