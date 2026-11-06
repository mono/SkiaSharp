// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

#if !NET8_0_OR_GREATER
using System;

namespace System.Diagnostics.CodeAnalysis;

// ExperimentalAttribute is built into .NET 8 and later.
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
internal sealed class ExperimentalAttribute : Attribute
{
	public ExperimentalAttribute(string diagnosticId) => DiagnosticId = diagnosticId;

	public string DiagnosticId { get; }

	public string? UrlFormat { get; set; }
}
#endif
