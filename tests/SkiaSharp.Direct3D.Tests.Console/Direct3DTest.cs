using System;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Direct3D.Tests;

public class Direct3DTest : SKTest
{
}
	
public class Direct3DTest<TContext> : Direct3DTest
	where TContext : Direct3DContext, new()
{
	protected Direct3DContext CreateDirect3DContext()
	{
		try
		{
			if (!IsWindows)
				throw new PlatformNotSupportedException();

			return new TContext();
		}
		catch (Exception ex)
		{
			throw new SkipException($"Unable to create Direct3D context: {ex.Message}");
		}
	}
}
