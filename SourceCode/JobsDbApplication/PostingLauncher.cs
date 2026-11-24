/////////////////////////////////////////////////////////////////////////////
// <copyright file="PostingLauncher.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Application;

using System;
using System.Diagnostics;

/// <summary>Passes a validated web address to the default browser.</summary>
internal static class PostingLauncher
{
	/// <summary>
	/// Passes the posting address to the system's default browser.
	/// </summary>
	/// <param name="address">The validated absolute posting address.</param>
	public static void Open(Uri address)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo();
		startInfo.FileName = address.AbsoluteUri;
		startInfo.UseShellExecute = true;
		using Process? process = Process.Start(startInfo);
	}
}
