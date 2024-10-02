namespace CouchbaseMauiPOC.Services;

public class MediaService : IMediaService
{
    public async Task<byte[]?> PickPhotoAsync()
    {
        if(DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            var status = await Permissions.RequestAsync<Permissions.Media>();
            if(status == PermissionStatus.Granted)
            {
                return await GetPhotoFromMedia();
            }
            else
            {
                return null;
            }
        }
        else
        {
            return await GetPhotoFromMedia();
        }
    }

    private async Task<byte[]?> GetPhotoFromMedia()
    {
        var result = await MediaPicker.PickPhotoAsync();
        if(result != null)
        {
            var stream = await result.OpenReadAsync();
            return GetBytesFromStream(stream);
        }

        return null;
    }

    private byte[] GetBytesFromStream(Stream stream)
    {
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
