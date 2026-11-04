using Acme.Center.Platform.Publishing.Domain.Model.Entities;
using Acme.Center.Platform.Publishing.Domain.Model.Events;
using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Publishing.Domain.Model.Aggregates;

/// <summary>
///     Partial class for the <see cref="Tutorial" /> aggregate, focusing on content management and publishing status.
/// </summary>
public partial class Tutorial : IPublishable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Tutorial" /> aggregate with default values.
    ///     This constructor is primarily for persistence mechanisms.
    /// </summary>
    public Tutorial()
    {
        Title = string.Empty; // Initialized in the other partial part, but good for safety
        Summary = string.Empty; // Initialized in the other partial part, but good for safety
        Assets = new List<Asset>();
        Status = EPublishingStatus.Draft;
    }

    /// <summary>
    ///     Gets the collection of <see cref="Asset" /> entities associated with this tutorial.
    /// </summary>
    public ICollection<Asset> Assets { get; }

    /// <summary>
    ///     Gets or sets the current publishing status of the tutorial.
    /// </summary>
    public EPublishingStatus Status { get; protected set; }

    /// <summary>
    ///     Indicates whether the tutorial has any readable assets.
    /// </summary>
    public bool Readable => HasReadableAssets;

    /// <summary>
    ///     Indicates whether the tutorial has any viewable assets.
    /// </summary>
    public bool Viewable => HasViewableAssets;

    /// <summary>
    ///     Determines if the tutorial contains any assets marked as readable.
    /// </summary>
    public bool HasReadableAssets => Assets.Any(asset => asset.Readable);

    /// <summary>
    ///     Determines if the tutorial contains any assets marked as viewable.
    /// </summary>
    public bool HasViewableAssets => Assets.Any(asset => asset.Viewable);

    /// <summary>
    ///     Transitions the tutorial's status to <see cref="EPublishingStatus.ReadyToEdit" />
    ///     if all associated assets are also in the <see cref="EPublishingStatus.ReadyToEdit" /> status.
    /// </summary>
    public void SendToEdit()
    {
        if (HasAllAssetsWithStatus(EPublishingStatus.ReadyToEdit))
            Status = EPublishingStatus.ReadyToEdit;
    }

    /// <summary>
    ///     Transitions the tutorial's status to <see cref="EPublishingStatus.ReadyToApproval" />
    ///     if all associated assets are also in the <see cref="EPublishingStatus.ReadyToApproval" /> status.
    /// </summary>
    public void SendToApproval()
    {
        if (HasAllAssetsWithStatus(EPublishingStatus.ReadyToApproval))
            Status = EPublishingStatus.ReadyToApproval;
    }

    /// <summary>
    ///     Approves and locks the tutorial, transitioning its status to <see cref="EPublishingStatus.ApprovedAndLocked" />.
    ///     This occurs only if all associated assets are also in the <see cref="EPublishingStatus.ApprovedAndLocked" />
    ///     status.
    /// </summary>
    public void ApproveAndLock()
    {
        if (HasAllAssetsWithStatus(EPublishingStatus.ApprovedAndLocked))
        {
            Status = EPublishingStatus.ApprovedAndLocked;
            AddDomainEvent(new TutorialPublishedEvent(Id));
        }
    }

    /// <summary>
    ///     Rejects the tutorial, transitioning its status back to <see cref="EPublishingStatus.Draft" />.
    /// </summary>
    public void Reject()
    {
        Status = EPublishingStatus.Draft;
    }

    /// <summary>
    ///     Returns the tutorial to the <see cref="EPublishingStatus.ReadyToEdit" /> status.
    /// </summary>
    public void ReturnToEdit()
    {
        Status = EPublishingStatus.ReadyToEdit;
    }

    /// <summary>
    ///     Checks if an image asset with the given URL already exists within the tutorial.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to check.</param>
    /// <returns><c>true</c> if the image asset exists; otherwise, <c>false</c>.</returns>
    private bool ExistsImageByUrl(string imageUrl)
    {
        return Assets.Any(asset => asset.Type == EAssetType.Image &&
                                   (string)asset.GetContent() == imageUrl);
    }

    /// <summary>
    ///     Checks if a video asset with the given URL already exists within the tutorial.
    /// </summary>
    /// <param name="videoUrl">The URL of the video to check.</param>
    /// <returns><c>true</c> if the video asset exists; otherwise, <c>false</c>.</returns>
    private bool ExistsVideoByUrl(string videoUrl)
    {
        return Assets.Any(asset => asset.Type == EAssetType.Video &&
                                   (string)asset.GetContent() == videoUrl);
    }

    /// <summary>
    ///     Checks if a readable content asset with the given content already exists within the tutorial.
    /// </summary>
    /// <param name="content">The readable content string to check.</param>
    /// <returns><c>true</c> if the readable content asset exists; otherwise, <c>false</c>.</returns>
    private bool ExistsReadableContent(string content)
    {
        return Assets.Any(asset => asset.Type == EAssetType.ReadableContentItem &&
                                   (string)asset.GetContent() == content);
    }

    /// <summary>
    ///     Determines if all assets associated with the tutorial currently have the specified publishing status.
    /// </summary>
    /// <param name="status">The <see cref="EPublishingStatus" /> to check against.</param>
    /// <returns><c>true</c> if all assets have the specified status; otherwise, <c>false</c>.</returns>
    private bool HasAllAssetsWithStatus(EPublishingStatus status)
    {
        return Assets.All(asset => asset.Status == status);
    }

    /// <summary>
    ///     Adds a new image asset to the tutorial if it does not already exist.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to add.</param>
    public void AddImage(string imageUrl)
    {
        if (ExistsImageByUrl(imageUrl)) return;
        Assets.Add(new ImageAsset(imageUrl));
    }

    /// <summary>
    ///     Adds a new video asset to the tutorial if it does not already exist.
    /// </summary>
    /// <param name="videoUrl">The URL of the video to add.</param>
    public void AddVideo(string videoUrl)
    {
        if (ExistsVideoByUrl(videoUrl)) return;
        Assets.Add(new VideoAsset(videoUrl));
    }

    /// <summary>
    ///     Adds a new readable content asset to the tutorial if it does not already exist.
    /// </summary>
    /// <param name="content">The readable content string to add.</param>
    public void AddReadableContent(string content)
    {
        if (ExistsReadableContent(content)) return;
        Assets.Add(new ReadableContentAsset(content));
    }

    /// <summary>
    ///     Removes a specific asset from the tutorial based on its unique identifier.
    /// </summary>
    /// <param name="identifier">The <see cref="AcmeAssetIdentifier" /> of the asset to remove.</param>
    public void RemoveAsset(AcmeAssetIdentifier identifier)
    {
        var asset = Assets.FirstOrDefault(a => a.AssetIdentifier == identifier);
        if (asset is null) return;
        Assets.Remove(asset);
    }

    /// <summary>
    ///     Clears all assets from the tutorial.
    /// </summary>
    public void ClearAssets()
    {
        Assets.Clear();
    }

    /// <summary>
    ///     Constructs and returns a list of <see cref="ContentItem" /> representing the tutorial's current assets.
    /// </summary>
    /// <remarks>
    ///     Each <see cref="ContentItem" /> includes the asset's type and its content as a string.
    /// </remarks>
    /// <returns>A <see cref="List{ContentItem}" /> of the tutorial's assets.</returns>
    public List<ContentItem> GetContent()
    {
        var content = new List<ContentItem>();
        if (Assets.Count > 0)
            content.AddRange(Assets.Select(asset =>
                new ContentItem(asset.Type.ToString(), asset.GetContent() as string ?? string.Empty)));
        return content;
    }
}