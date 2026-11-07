using System.Net;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>Response of the fetch of a live result : body, status and rate limit state of the fetch.</summary>
internal readonly record struct LiveFetchResponse(byte[] Body, HttpStatusCode Status, RateLimitInfo Limit)
{
    internal bool IsSuccess => (int)Status is >= 200 and < 300;
}
