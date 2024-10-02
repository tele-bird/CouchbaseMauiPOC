using System;

namespace CouchbaseMauiPOC.Services;

public interface IMediaService
{
    Task<byte[]?> PickPhotoAsync();
}
