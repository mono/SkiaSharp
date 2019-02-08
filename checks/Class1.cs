using System;

namespace checks
{
	public class Class1
	{
		public unsafe void Test()
		{
			int* arr = stackalloc [] { 1, 2, 3 };
		}
	}
}
