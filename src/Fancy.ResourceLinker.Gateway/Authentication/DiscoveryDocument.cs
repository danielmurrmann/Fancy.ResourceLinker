﻿using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Gateway.Authentication;

/// <summary>
/// A class to describe a discovery document
/// </summary>
public class DiscoveryDocument
{
    /// <summary>
    /// Gets or sets the token endpoint.
    /// </summary>
    /// <value>
    /// The token endpoint.
    /// </value>
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; } = "";

    /// <summary>
    /// Gets or sets the userinfo endpoint.
    /// </summary>
    /// <value>
    /// The userinfo endpoint.
    /// </value>
    [JsonPropertyName("userinfo_endpoint")]
    public string UserinfoEndpoint { get; set; } = "";
}
