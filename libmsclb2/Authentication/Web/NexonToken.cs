using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Authentication.Web
{
    /// <summary>
    /// Token used for authentication in Nexon games
    /// </summary>
    public sealed class NexonToken
    {
        /// <summary>
        /// The actual token
        /// </summary>
        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string RefreshToken { get; set; }
    }
}
