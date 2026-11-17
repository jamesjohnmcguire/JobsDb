/////////////////////////////////////////////////////////////////////////////
// <copyright file="JobStorageMapper.cs" company="Digital Zen Works">
// Copyright © 2024 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.JobsDb.Library.Persistence;

using System;
using DigitalZenWorks.JobsDb.Library.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DomainJob = DigitalZenWorks.JobsDb.Library.Models.Job;
using StorageJob = DigitalZenWorks.JobsDb.Library.Persistence.Generated.Job;

/// <summary>
/// Converts SQL-generated storage values at the persistence boundary.
/// </summary>
public static class JobStorageMapper
{
	// Reuse EF formats for compatibility with existing databases.
	private static readonly DateTimeToStringConverter DateConverter =
		new DateTimeToStringConverter();

	private static readonly NumberToStringConverter<decimal> SalaryConverter =
		new NumberToStringConverter<decimal>();

	private static readonly BoolToZeroOneConverter<int> BooleanConverter =
		new BoolToZeroOneConverter<int>();

	private static readonly EnumToStringConverter<ApplicationStatus>
		StatusConverter = new EnumToStringConverter<ApplicationStatus>();

	/// <summary>
	/// Converts a stored row into an application job.
	/// </summary>
	/// <param name="storedJob">The SQL-generated storage row.</param>
	/// <returns>An independent application job.</returns>
	public static DomainJob ToDomain(StorageJob storedJob)
	{
		ArgumentNullException.ThrowIfNull(storedJob);

		return new DomainJob
		{
			Id = storedJob.Id,
			Title = storedJob.Title,
			Company = storedJob.Company,
			Location = storedJob.Location,
			Description = storedJob.Description,
			Requirements = storedJob.Requirements,
			Source = storedJob.Source,
			SourceUrl = storedJob.SourceUrl,
			SourceJobId = storedJob.SourceJobId,
			SalaryMin = ReadSalary(storedJob.SalaryMin),
			SalaryMax = ReadSalary(storedJob.SalaryMax),
			SalaryCurrency = storedJob.SalaryCurrency,
			JobType = storedJob.JobType,
			RemoteType = storedJob.RemoteType,
			DatePosted = DateConverter.ConvertFromProviderTyped(
				storedJob.DatePosted),
			DateScraped = DateConverter.ConvertFromProviderTyped(
				storedJob.DateScraped),
			DateApplied = ReadDate(storedJob.DateApplied),
			Status = StatusConverter.ConvertFromProviderTyped(storedJob.Status),
			Priority = storedJob.Priority,
			Notes = storedJob.Notes,
			IsArchived = BooleanConverter.ConvertFromProviderTyped(
				storedJob.IsArchived)
		};
	}

	/// <summary>
	/// Converts an application job into a storage row.
	/// </summary>
	/// <param name="job">The application job.</param>
	/// <returns>An independent SQL-generated storage row.</returns>
	public static StorageJob ToStorage(DomainJob job)
	{
		ArgumentNullException.ThrowIfNull(job);

		return new StorageJob
		{
			Id = job.Id,
			Title = job.Title,
			Company = job.Company,
			Location = job.Location,
			Description = job.Description,
			Requirements = job.Requirements,
			Source = job.Source,
			SourceUrl = job.SourceUrl,
			SourceJobId = job.SourceJobId,
			SalaryMin = WriteSalary(job.SalaryMin),
			SalaryMax = WriteSalary(job.SalaryMax),
			SalaryCurrency = job.SalaryCurrency,
			JobType = job.JobType,
			RemoteType = job.RemoteType,
			DatePosted = DateConverter.ConvertToProviderTyped(job.DatePosted),
			DateScraped = DateConverter.ConvertToProviderTyped(job.DateScraped),
			DateApplied = WriteDate(job.DateApplied),
			Status = StatusConverter.ConvertToProviderTyped(job.Status),
			Priority = job.Priority,
			Notes = job.Notes,
			IsArchived = BooleanConverter.ConvertToProviderTyped(job.IsArchived)
		};
	}

	private static decimal? ReadSalary(string? salary)
	{
		if (salary == null)
		{
			return null;
		}

		return SalaryConverter.ConvertFromProviderTyped(salary);
	}

	private static string? WriteSalary(decimal? salary)
	{
		if (salary == null)
		{
			return null;
		}

		return SalaryConverter.ConvertToProviderTyped(salary.Value);
	}

	private static DateTime? ReadDate(string? date)
	{
		if (date == null)
		{
			return null;
		}

		return DateConverter.ConvertFromProviderTyped(date);
	}

	private static string? WriteDate(DateTime? date)
	{
		if (date == null)
		{
			return null;
		}

		return DateConverter.ConvertToProviderTyped(date.Value);
	}
}
