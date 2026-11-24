/////////////////////////////////////////////////////////////////////////////
// <copyright file="UnitTest1.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using NUnit.Framework;

/// <summary>
/// Provides a basic NUnit discovery and execution check.
/// </summary>
internal class Tests
{
	/// <summary>
	/// Runs before the smoke test.
	/// </summary>
	[SetUp]
	public void Setup()
	{
	}

	/// <summary>
	/// Confirms that the fixture can execute under NUnit.
	/// </summary>
	[Test]
	public void Test1()
	{
		Assert.Pass();
	}
}
