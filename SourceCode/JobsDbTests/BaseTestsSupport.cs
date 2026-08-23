/////////////////////////////////////////////////////////////////////////////
// <copyright file="BaseTestsSupport.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace JobsDbTests;

using System;
using System.IO;
using DigitalZenWorks.JobsDb.Library.Models;
using NUnit.Framework;

/// <summary>
/// Base test support class.
/// </summary>
internal abstract class BaseTestsSupport : IDisposable
{
	private bool disposed;

	/// <summary>
	/// Gets or sets the test database path.
	/// </summary>
	protected string? TestDbPath { get; set; }

	/// <summary>
	/// Gets or sets the test job.
	/// </summary>
	protected Job? TestJob { get; set; }

	/// <summary>
	/// The one time setup method.
	/// </summary>
	[OneTimeSetUp]
	public void BaseOneTimeSetUp()
	{
		TestDbPath =
			Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");

		TestJob = new Job
		{
			Title = "Test",
			Company = "Test Company",
			Description = "Test Description",
			JobType = "Full-time",
			Notes = "Test Notes",
			Location = "Remote",
			Requirements = "None",
			RemoteType = "Remote",
			SalaryCurrency = "USD",
			Source = "Test",
			SourceUrl = "https://test.com",
			SourceJobId = "123",
			DatePosted = DateTime.UtcNow
		};
	}

	/// <summary>
	/// One time tear down method.
	/// </summary>
	[OneTimeTearDown]
	public void BaseOneTimeTearDown()
	{
		if (File.Exists(TestDbPath))
		{
			File.Delete(TestDbPath);
		}
	}

	/// <summary>
	/// Dispose method.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Copy a job object.
	/// </summary>
	/// <param name="job">The job to be copied.</param>
	/// <returns>The copied job.</returns>
	protected static Job CopyJob(Job job)
	{
		Job copyJob = new Job
		{
			Title = job.Title,
			Company = job.Company,
			Description = job.Description,
			JobType = job.JobType,
			Notes = job.Notes,
			Location = job.Location,
			Requirements = job.Requirements,
			RemoteType = job.RemoteType,
			SalaryCurrency = job.SalaryCurrency,
			Source = job.Source,
			SourceUrl = job.SourceUrl,
			SourceJobId = job.SourceJobId,
			DatePosted = job.DatePosted,
			DateScraped = job.DateScraped,
			DateApplied = job.DateApplied,
			Status = job.Status,
			Priority = job.Priority,
			IsArchived = job.IsArchived
		};

		return copyJob;
	}

	/// <summary>
	/// Dispose method.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged
	/// resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing == true && disposed == false)
		{
		}

		disposed = true;
	}
}
