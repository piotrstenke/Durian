// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;

[JsonObject]
internal sealed class DiagnosticData
{
    [JsonProperty("documentation")]
    public string? Documentation { get; set; }

    [JsonProperty("fatal")]
    public bool Fatal { get; set; }

    [JsonProperty("file", Required = Required.Always)]
    public string? File { get; set; }

    [JsonProperty("hasLocation")]
    public bool HasLocation { get; set; }

    [JsonProperty("id", Required = Required.Always)]
    public string? Id { get; set; }

    [JsonProperty("moduleName", Required = Required.Always)]
    public string? ModuleName { get; set; }

    [JsonProperty("title", Required = Required.Always)]
    public string? Title { get; set; }
}