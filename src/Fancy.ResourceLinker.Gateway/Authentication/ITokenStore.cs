﻿namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// Interface to a token store.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Saves the or update tokens asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="idToken">The identifier token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="expiresAt">The expires at.</param>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    Task SaveOrUpdateTokensAsync(string sessionId, string idToken, string accessToken, string refreshToken, DateTime expiresAt);

    /// <summary>
    /// Saves the or update userinfo claims asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userinfoClaims">The userinfo object as json string.</param>
    /// <returns>
    /// A task indicating the completion of the asynchronous operation.
    /// </returns>
    Task SaveOrUpdateUserinfoClaimsAsync(string sessionId, string userinfoClaims);

    /// <summary>
    /// Gets the token record for a provided session asynchronous.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A token record if available.
    /// </returns>
    Task<TokenRecord?> GetTokenRecordAsync(string sessionId);

    /// <summary>
    /// Cleans up the expired token records asynchronous.
    /// </summary>
    /// <returns>A task indicating the completion of the asynchronous operation.</returns>
    Task CleanupExpiredTokenRecordsAsync();
}