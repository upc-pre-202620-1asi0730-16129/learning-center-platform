using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Publishing.Domain.Model.Entities;

/// <summary>
///     Represents a video asset in the ACME Learning Center Platform.
/// </summary>
public class VideoAsset : Asset
{
    public VideoAsset() : base(EAssetType.Video)
    {
        VideoUri = null;
    }

    public VideoAsset(string videoUrl) : base(EAssetType.Video)
    {
        VideoUri = new Uri(videoUrl);
    }

    public Uri? VideoUri { get; private set; }
    public override bool Readable => false;
    public override bool Viewable => true;
}