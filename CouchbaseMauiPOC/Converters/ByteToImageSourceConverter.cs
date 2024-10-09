using System;
using System.Diagnostics;
using System.Globalization;

namespace CouchbaseMauiPOC.Converters;

public class ByteToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ImageSource? imageSource = null;
        try
        {
            if(value != null)
            {
                imageSource = ImageSource.FromStream(() => new MemoryStream((byte[])value));
            }
        }
        catch(Exception exc)
        {
            Trace.WriteLine($"{nameof(ByteToImageSourceConverter)}.{nameof(Convert)} caught an {exc.GetType().Name}: {exc.Message}");
        }

        return imageSource ?? "profile_placeholder.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
