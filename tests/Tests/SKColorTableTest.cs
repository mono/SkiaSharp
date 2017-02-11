using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKColorTableTest : SKTest
	{
		private readonly static SKColor[] Colors = new SKColor[] 
		{ 
			SKColors.Black, 
			SKColors.White, 
			SKColors.Red, 
			SKColors.Green, 
			SKColors.Blue 
		};

		[Fact]
		public void MembersRetrieveColors()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Equal(Colors.Length, colorTable.Count);

			Assert.Equal(Colors, colorTable.Colors);

			Assert.Equal(Colors[0], colorTable[0]);
			Assert.Equal(Colors[1], colorTable[1]);
			Assert.Equal(Colors[2], colorTable[2]);
			Assert.Equal(Colors[3], colorTable[3]);
			Assert.Equal(Colors[4], colorTable[4]);
		}

		[Fact]
		public void IndexerOutOfRangeBelow()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[-1];
			});
		}

		[Fact]
		public void IndexerOutOfRangeAbove()
		{
			var colorTable = new SKColorTable(Colors);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var color = colorTable[5];
			});
		}
	}
}
