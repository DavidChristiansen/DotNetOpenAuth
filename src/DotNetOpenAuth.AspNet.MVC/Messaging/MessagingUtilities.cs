//-----------------------------------------------------------------------
// <copyright file="MessagingUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.MVC.Messaging {
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    using DotNetOpenAuth.Messaging;

    /// <summary>
	/// A grab-bag of utility methods useful for the channel stack of the protocol.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Utility class touches lots of surface area")]
	public static class MessagingUtilities {

        /// <summary>
        /// Transforms an OutgoingWebResponse to an MVC-friendly ActionResult.
        /// </summary>
        /// <param name="response">The response to send to the user agent.</param>
        /// <returns>The <see cref="ActionResult"/> instance to be returned by the Controller's action method.</returns>
        public static ActionResult AsActionResult(this OutgoingWebResponse response) {
            Requires.NotNull(response, "response");
            return new OutgoingWebResponseActionResult(response);
        }

	}
}
