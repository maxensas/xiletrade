using System;
using System.Net;

namespace AutoUpdaterDotNET
{
    /// <inheritdoc />
#pragma warning disable SYSLIB0014 // Le type ou le membre est obsolète
    public class MyWebClient : WebClient
    {
        /// <summary>
        ///     Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri;

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }
    }
#pragma warning restore SYSLIB0014 // Le type ou le membre est obsolète
}
