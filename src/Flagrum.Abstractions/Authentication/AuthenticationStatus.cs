namespace Flagrum.Abstractions;

/// <summary>
/// The status of the user's authentication with the Lucent server.
/// </summary>
public enum AuthenticationStatus
{
    /// <summary>
    /// The user is not authenticated.
    /// </summary>
    Unauthenticated,

    /// <summary>
    /// The user has been authenticated via gift token.
    /// </summary>
    Gifted,

    /// <summary>
    /// The user's gift token has expired, so they are no longer authenticated.
    /// </summary>
    GiftTokenExpired,

    /// <summary>
    /// The user has been authenticated as a subscriber.
    /// </summary>
    Subscriber,

    /// <summary>
    /// The user does not have a high enough subscription tier, so they are not authenticated.
    /// </summary>
    SubscriptionTierTooLow,

    /// <summary>
    /// The user authenticated via a premium account, but does not belong to any of the tiers, so they are not authenticated.
    /// </summary>
    NoSubscriptionTier,

    /// <summary>
    /// An error occurred while trying to determine the authentication status.
    /// </summary>
    Errored
}