using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Disqord.Models;
using Qommon;

namespace Disqord.Gateway;

public class CachedSharedUser : CachedUser, ICachedSharedUser
{
    /// <inheritdoc/>
    public override string Name => _name;

    /// <inheritdoc/>
    [Obsolete(Pomelo.DiscriminatorObsoletion)]
    public override string Discriminator => _discriminator.ToString("0000", CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override string? GlobalName => _globalName;

    /// <inheritdoc/>
    public override string? AvatarHash => _avatarHash;

    /// <inheritdoc/>
    public override bool IsBot => _isBot;

    /// <inheritdoc/>
    public override UserFlags PublicFlags => _publicFlags;

    /// <inheritdoc/>
    public override IUserPrimaryGuild? PrimaryGuild => _primaryGuild;

    /// <inheritdoc/>
    public override IAvatarDecoration? AvatarDecoration => _avatarDecoration;

    /// <inheritdoc/>
    public override ICollectibles? Collectibles => _collectibles;

    /// <inheritdoc/>
    public int ReferenceCount => _referenceCount;

    private string _name = null!;
    private string? _globalName;
    private short _discriminator;
    private string? _avatarHash;
    private readonly bool _isBot;
    private UserFlags _publicFlags;
    private IUserPrimaryGuild? _primaryGuild;
    private IAvatarDecoration? _avatarDecoration;
    private ICollectibles? _collectibles;
    private int _referenceCount;

    /// <summary>
    ///     Instantiates a new shared user.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="model"></param>
    public CachedSharedUser(IGatewayClient client, UserJsonModel model)
        : base(client, model)
    {
        _isBot = model.Bot.GetValueOrDefault();

        Update(model);
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Update(UserJsonModel model)
    {
        _name = model.Username;
        _globalName = model.GlobalName;
        _discriminator = model.Discriminator;
        _avatarHash = model.Avatar;

        if (model.PublicFlags.HasValue)
            _publicFlags = model.PublicFlags.Value;

        if (model.PrimaryGuild.HasValue)
            _primaryGuild = model.PrimaryGuild.Value != null ? new TransientUserPrimaryGuild(model.PrimaryGuild.Value) : null;

        if (model.AvatarDecorationData.HasValue)
            _avatarDecoration = model.AvatarDecorationData.Value != null ? new TransientAvatarDecoration(model.AvatarDecorationData.Value) : null;

        if (model.Collectibles.HasValue)
            _collectibles = model.Collectibles.Value != null ? new TransientCollectibles(model.Collectibles.Value) : null;
    }

    /// <inheritdoc/>
    public int AddReference(CachedUser user)
    {
        return Interlocked.Increment(ref _referenceCount);
    }

    /// <inheritdoc/>
    public int RemoveReference(CachedUser user)
    {
        return Interlocked.Decrement(ref _referenceCount);
    }
}
