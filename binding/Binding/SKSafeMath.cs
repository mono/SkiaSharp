using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	// https://wiki.sei.cmu.edu/confluence/display/c/typeof(int)32-C.+Ensure+that+operations+on+signed+integers+do+not+result+in+overflow
	public struct SKSafeMath
	{
		private bool fOK;

		private bool throwOnError;

		/// <summary>
		/// <inheritdoc cref="Ok"/>
		/// </summary>
		public bool ThrowOnError { get => throwOnError; set => throwOnError = value; }

		/// <summary>
		/// <br></br> all methods detect overflow BEFORE computing the return value
		/// <br></br>
		/// <br></br> IMPORTANT: the following behaviour applies to when ThrowOnError is set to true:
		/// <br></br>
		/// <br></br>
		/// <br></br> when a method detects overflow, an OverflowException will be thrown
		/// <br></br>
		/// <br></br>
		/// <br></br> IMPORTANT: the following behaviour applies to when ThrowOnError is set to false:
		/// <br></br>
		/// <br></br>
		/// <br></br> when a method detects overflow, it will set this flag to false and return a value of default(T)
		/// <br></br>
		/// <br></br> all following methods invoked by this instance will immediately fail and return a value of default(T)
		/// <br></br>
		/// <br></br>
		/// <br></br> overflow can be tested by either of the following
		/// <br></br> bool overflow1 = !safe.Ok; // option 1
		/// <br></br> bool overflow2 = !safe;    // option 2
		/// </summary>
		/// <returns>
		/// the status of the overflow detector
		/// <br></br> - [false = overflow]
		/// <br></br> - [true = no overflow]
		/// </returns>
		/// <see cref="ThrowOnError"/>
		public bool Ok => fOK;


		/// <summary>
		/// <inheritdoc cref="Ok"/>
		/// </summary>
		public SKSafeMath (bool ThrowOnError = false)
		{
			fOK = true;
			throwOnError = ThrowOnError;
		}

		/// <summary>
		/// <inheritdoc cref="Ok"/>
		/// </summary>
		public SKSafeMath ()
		{
			fOK = true;
			throwOnError = false;
		}

		static T1 UnsafeAs<T, T1> (T value)
			where T1 : unmanaged
			where T : unmanaged
		{
			unsafe {
				return *(T1*)&value;
			}
		}

		private void Fail (string msg)
		{
			if (throwOnError) {
				throw new OverflowException (msg);
			} else {
				LOG (msg);
				fOK = false;
			}
		}

		private static void LOG (string msg)
		{
			// do nothing for now
		}

		public static implicit operator bool (SKSafeMath safeMath)
		{
			return safeMath.fOK;
		}

		public static bool TryAdd<T> (T x, T y, out T result) where T : unmanaged
		{
			SKSafeMath safe = new ();
			result = safe.Add (x, y);
			return safe;
		}

		public static bool TrySub<T> (T x, T y, out T result) where T : unmanaged
		{
			SKSafeMath safe = new ();
			result = safe.Sub (x, y);
			return safe;
		}

		public static bool TryMul<T> (T x, T y, out T result) where T : unmanaged
		{
			SKSafeMath safe = new ();
			result = safe.Mul (x, y);
			return safe;
		}

		public static bool TryDiv<T> (T x, T y, out T result) where T : unmanaged
		{
			SKSafeMath safe = new ();
			result = safe.Div (x, y);
			return safe;
		}

		public static bool TryAlignUp<T> (T x, T alignment, out T result) where T : unmanaged
		{
			SKSafeMath safe = new ();
			result = safe.AlignUp (x, alignment);
			return safe;
		}

		public static bool TryAlign4<T> (T x, out T result) where T : unmanaged
		{
			SKSafeMath safe = new SKSafeMath ();

			object? alignObj = Convert.ChangeType (4, typeof (T));

			if (alignObj == null) {
				safe.Fail ("could not convert 4 (" + typeof (int).FullName + ") to T (" + typeof (T).FullName + ")");
			}

			T alignment = (T)alignObj;

			return TryAlignUp<T> (x, alignment, out result);
		}

		public static bool TryAlign8<T> (T x, out T result) where T : unmanaged
		{
			SKSafeMath safe = new SKSafeMath ();

			object? alignObj = Convert.ChangeType (8, typeof (T));

			if (alignObj == null) {
				safe.Fail ("could not convert 4 (" + typeof (int).FullName + ") to T (" + typeof (T).FullName + ")");
			}

			T alignment = (T)alignObj;

			return TryAlignUp<T> (x, alignment, out result);
		}

		[System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Signed<T> ()
			=> typeof (T) == typeof (sbyte)
			|| typeof (T) == typeof (short)
			|| typeof (T) == typeof (int)
			|| typeof (T) == typeof (nint)
			|| typeof (T) == typeof (long)
			|| typeof (T) == typeof (char)
			;

		private static T Throw<T> ()
		{
			throw new InvalidCastException ("Only Integer types are supported: sbyte, byte, char, uint, int, ulong, long");
		}

		public unsafe T Add<T> (T x_, T y_) where T : unmanaged
		{
			if (!fOK)
				return default;
			if (sizeof (T) == 1 && typeof (T) != typeof (bool)) {
				byte x = UnsafeAs<T, byte> (x_);
				byte y = UnsafeAs<T, byte> (y_);
				string str = "Addition would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						((sbyte)y > 0) && ((sbyte)x > (sbyte.MaxValue - (sbyte)y))
						|| (((sbyte)y < 0) && ((sbyte)x < (sbyte.MinValue - (sbyte)y)))
					) {
						Fail (str);
						return default;
					}
				} else if (byte.MaxValue - x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<byte, T> ((byte)(x + y));
			} else if (sizeof (T) == 2) {
				ushort x = UnsafeAs<T, ushort> (x_);
				ushort y = UnsafeAs<T, ushort> (y_);
				string str = "Addition would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						((short)y > 0) && ((short)x > (short.MaxValue - (short)y))
						|| (((short)y < 0) && ((short)x < (short.MinValue - (short)y)))
					) {
						Fail (str);
						return default;
					}
				} else if (ushort.MaxValue - x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<ushort, T> ((ushort)(x + y));
			} else if (sizeof (T) == 4 && typeof (T) != typeof (float)) {
				uint x = UnsafeAs<T, uint> (x_);
				uint y = UnsafeAs<T, uint> (y_);
				string str = "Addition would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						((int)y > 0) && ((int)x > (int.MaxValue - (int)y))
						|| (((int)y < 0) && ((int)x < (int.MinValue - (int)y)))
					) {
						Fail (str);
						return default;
					}
				} else if (uint.MaxValue - x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<uint, T> ((uint)(x + y));
			} else if (sizeof (T) == 8 && typeof (T) != typeof (double)) {
				ulong x = UnsafeAs<T, ulong> (x_);
				ulong y = UnsafeAs<T, ulong> (y_);
				string str = "Addition would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						((long)y > 0) && ((long)x > (long.MaxValue - (long)y))
						|| (((long)y < 0) && ((long)x < (long.MinValue - (long)y)))
					) {
						Fail (str);
						return default;
					}
				} else if (ulong.MaxValue - x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<ulong, T> ((ulong)(x + y));
			} else {
				return Throw<T> ();
			}
		}

		public unsafe T Sub<T> (T x_, T y_) where T : unmanaged
		{
			if (!fOK)
				return default;
			if (sizeof (T) == 1 && typeof (T) != typeof (bool)) {
				byte x = UnsafeAs<T, byte> (x_);
				byte y = UnsafeAs<T, byte> (y_);
				string str = "Subtraction would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						(((sbyte)y > 0) && ((sbyte)x < (sbyte.MinValue + (sbyte)y)))
						|| ((sbyte)y < 0) && ((sbyte)x > (sbyte.MaxValue + (sbyte)y))
					) {
						Fail (str);
						return default;
					}
				} else if (x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<byte, T> ((byte)(x - y));
			} else if (sizeof (T) == 2) {
				ushort x = UnsafeAs<T, ushort> (x_);
				ushort y = UnsafeAs<T, ushort> (y_);
				string str = "Subtraction would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						(((short)y > 0) && ((short)x < (short.MinValue + (short)y)))
						|| ((short)y < 0) && ((short)x > (short.MaxValue + (short)y))
					) {
						Fail (str);
						return default;
					}
				} else if (x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<ushort, T> ((ushort)(x - y));
			} else if (sizeof (T) == 4 && typeof (T) != typeof (float)) {
				uint x = UnsafeAs<T, uint> (x_);
				uint y = UnsafeAs<T, uint> (y_);
				string str = "Subtraction would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						(((int)y > 0) && ((int)x < (int.MinValue + (int)y)))
						|| ((int)y < 0) && ((int)x > (int.MaxValue + (int)y))
					) {
						Fail (str);
						return default;
					}
				} else if (x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<uint, T> ((uint)(x - y));
			} else if (sizeof (T) == 8 && typeof (T) != typeof (double)) {
				ulong x = UnsafeAs<T, ulong> (x_);
				ulong y = UnsafeAs<T, ulong> (y_);
				string str = "Subtraction would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if (
						(((long)y > 0) && ((long)x < (long.MinValue + (long)y)))
						|| ((long)y < 0) && ((long)x > (long.MaxValue + (long)y))
					) {
						Fail (str);
						return default;
					}
				} else if (x < y) {
					Fail (str);
					return default;
				}
				return UnsafeAs<ulong, T> ((ulong)(x - y));
			} else {
				return Throw<T> ();
			}
		}

		public unsafe T Mul<T> (T x_, T y_) where T : unmanaged
		{
			if (!fOK)
				return default;
			if (sizeof (T) == 1 && typeof (T) != typeof (bool)) {
				byte x = UnsafeAs<T, byte> (x_);
				byte y = UnsafeAs<T, byte> (y_);
				string str = "Multiplication would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if ((sbyte)x != 0 && (sbyte)y != 0) {
						if ((sbyte)x > 0) {  /* x is positive */
							if ((sbyte)y > 0) {  /* x and y are positive */
								if ((sbyte)x > (sbyte.MaxValue / (sbyte)y)) {
									Fail (str);
									return default;
								}
							} else { /* x positive, y nonpositive */
								if ((sbyte)y < (sbyte.MinValue / (sbyte)x)) {
									Fail (str);
									return default;
								}
							} /* x positive, y nonpositive */
						} else { /* x is nonpositive */
							if ((sbyte)y > 0) { /* x is nonpositive, y is positive */
								if ((sbyte)x < (sbyte.MinValue / (sbyte)y)) {
									Fail (str);
									return default;
								}
							} else { /* x and y are nonpositive */
								if (((sbyte)x != 0) && ((sbyte)y < (sbyte.MaxValue / (sbyte)x))) {
									Fail (str);
									return default;
								}
							} /* End if x and y are nonpositive */
						} /* End if x is nonpositive */
					}
				} else if (x != 0 && y != 0) {
					if (x > byte.MaxValue / y) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<byte, T> ((byte)(x * y));
			} else if (sizeof (T) == 2) {
				ushort x = UnsafeAs<T, ushort> (x_);
				ushort y = UnsafeAs<T, ushort> (y_);
				string str = "Multiplication would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if ((short)x != 0 && (short)y != 0) {
						if ((short)x > 0) {  /* x is positive */
							if ((short)y > 0) {  /* x and y are positive */
								if ((short)x > (short.MaxValue / (short)y)) {
									Fail (str);
									return default;
								}
							} else { /* x positive, y nonpositive */
								if ((short)y < (short.MinValue / (short)x)) {
									Fail (str);
									return default;
								}
							} /* x positive, y nonpositive */
						} else { /* x is nonpositive */
							if ((short)y > 0) { /* x is nonpositive, y is positive */
								if ((short)x < (short.MinValue / (short)y)) {
									Fail (str);
									return default;
								}
							} else { /* x and y are nonpositive */
								if (((short)x != 0) && ((short)y < (short.MaxValue / (short)x))) {
									Fail (str);
									return default;
								}
							} /* End if x and y are nonpositive */
						} /* End if x is nonpositive */
					}
				} else if (x != 0 && y != 0) {
					if (x > ushort.MaxValue / y) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<ushort, T> ((ushort)(x * y));
			} else if (sizeof (T) == 4 && typeof (T) != typeof (float)) {
				uint x = UnsafeAs<T, uint> (x_);
				uint y = UnsafeAs<T, uint> (y_);
				string str = "Multiplication would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if ((int)x != 0 && (int)y != 0) {
						if ((int)x > 0) {  /* x is positive */
							if ((int)y > 0) {  /* x and y are positive */
								if ((int)x > (int.MaxValue / (int)y)) {
									Fail (str);
									return default;
								}
							} else { /* x positive, y nonpositive */
								if ((int)y < (int.MinValue / (int)x)) {
									Fail (str);
									return default;
								}
							} /* x positive, y nonpositive */
						} else { /* x is nonpositive */
							if ((int)y > 0) { /* x is nonpositive, y is positive */
								if ((int)x < (int.MinValue / (int)y)) {
									Fail (str);
									return default;
								}
							} else { /* x and y are nonpositive */
								if (((int)x != 0) && ((int)y < (int.MaxValue / (int)x))) {
									Fail (str);
									return default;
								}
							} /* End if x and y are nonpositive */
						} /* End if x is nonpositive */
					}
				} else if (x != 0 && y != 0) {
					if (x > uint.MaxValue / y) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<uint, T> ((uint)(x * y));
			} else if (sizeof (T) == 8 && typeof (T) != typeof (double)) {
				ulong x = UnsafeAs<T, ulong> (x_);
				ulong y = UnsafeAs<T, ulong> (y_);
				string str = "Multiplication would overflow: x: " + x + ", y: " + y;
				if (Signed<T> ()) {
					if ((long)x != 0 && (long)y != 0) {
						if ((long)x > 0) {  /* x is positive */
							if ((long)y > 0) {  /* x and y are positive */
								if ((long)x > (long.MaxValue / (long)y)) {
									Fail (str);
									return default;
								}
							} else { /* x positive, y nonpositive */
								if ((long)y < (long.MinValue / (long)x)) {
									Fail (str);
									return default;
								}
							} /* x positive, y nonpositive */
						} else { /* x is nonpositive */
							if ((long)y > 0) { /* x is nonpositive, y is positive */
								if ((long)x < (long.MinValue / (long)y)) {
									Fail (str);
									return default;
								}
							} else { /* x and y are nonpositive */
								if (((long)x != 0) && ((long)y < (long.MaxValue / (long)x))) {
									Fail (str);
									return default;
								}
							} /* End if x and y are nonpositive */
						} /* End if x is nonpositive */
					}
				} else if (x != 0 && y != 0) {
					if (x > ulong.MaxValue / y) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<ulong, T> ((ulong)(x * y));
			} else {
				return Throw<T> ();
			}
		}

		public unsafe T Div<T> (T x_, T y_) where T : unmanaged
		{
			if (!fOK)
				return default;
			if (sizeof (T) == 1 && typeof (T) != typeof (bool)) {
				byte y = UnsafeAs<T, byte> (y_);
				if (y == 0) {
					Fail ("Cannot divide by zero");
					return default;
				}
				byte x = UnsafeAs<T, byte> (x_);
				string str = "Division would overflow: sbyte.MinValue / -1";
				if (Signed<T> ()) {
					if (((sbyte)x == sbyte.MinValue) && ((sbyte)y == -1)) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<byte, T> ((byte)(x / y));
			} else if (sizeof (T) == 2) {
				ushort y = UnsafeAs<T, ushort> (y_);
				if (y == 0) {
					Fail ("Cannot divide by zero");
					return default;
				}
				ushort x = UnsafeAs<T, ushort> (x_);
				string str = "Division would overflow: short.MinValue / -1";
				if (Signed<T> ()) {
					if (((short)x == short.MinValue) && ((short)y == -1)) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<ushort, T> ((ushort)(x / y));
			} else if (sizeof (T) == 4 && typeof (T) != typeof (float)) {
				uint y = UnsafeAs<T, uint> (y_);
				if (y == 0) {
					Fail ("Cannot divide by zero");
					return default;
				}
				uint x = UnsafeAs<T, uint> (x_);
				string str = "Division would overflow: int.MinValue / -1";
				if (Signed<T> ()) {
					if (((int)x == int.MinValue) && ((int)y == -1)) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<uint, T> ((uint)(x / y));
			} else if (sizeof (T) == 8 && typeof (T) != typeof (double)) {
				ulong y = UnsafeAs<T, ulong> (y_);
				if (y == 0) {
					Fail ("Cannot divide by zero");
					return default;
				}
				ulong x = UnsafeAs<T, ulong> (x_);
				string str = "Division would overflow: long.MinValue / -1";
				if (Signed<T> ()) {
					if (((long)x == long.MinValue) && ((long)y == -1)) {
						Fail (str);
						return default;
					}
				}
				return UnsafeAs<ulong, T> ((ulong)(x / y));
			} else {
				return Throw<T> ();
			}
		}

		public unsafe T AlignUp<T> (T x_, T alignment_) where T : unmanaged
		{
			if (!fOK)
				return default;
			if (sizeof (T) == 1 && typeof (T) != typeof (bool)) {
				byte alignment = UnsafeAs<T, byte> (alignment_);
				if (alignment == 0) {
					Fail ("Alignment must not be zero");
					return default;
				}
				if ((alignment & (alignment - 1)) != 0) {
					Fail ("Alignment must be a power of two (1, 2, 4, 8, 16, 32, ect) : alignment: " + alignment);
					return default;
				}
				byte x = UnsafeAs<T, byte> (x_);
				string str = "Alignment would overflow: x: " + x + ", alignment: " + alignment;
				if (Signed<T> ()) {
					if ((sbyte)alignment < 0) {
						Fail ("Alignment cannot be negative");
						return default;
					}

					if ((sbyte)x >= 0) {
						if ((sbyte)x > (sbyte.MaxValue - (sbyte)alignment)) {
							Fail (str);
							return default;
						}
					}

					sbyte a = (sbyte)((sbyte)alignment - 1);
					return UnsafeAs<byte, T> ((byte)(((sbyte)x + a) & ~a));
				} else {
					if (x >= 0) {
						if (x > (byte.MaxValue - alignment)) {
							Fail (str);
							return default;
						}
					}

					byte a = (byte)(alignment - 1);
					return UnsafeAs<byte, T> ((byte)((x + a) & ~a));
				}
			} else if (sizeof (T) == 2) {
				ushort alignment = UnsafeAs<T, ushort> (alignment_);
				if (alignment == 0) {
					Fail ("Alignment must not be zero");
					return default;
				}
				if ((alignment & (alignment - 1)) != 0) {
					Fail ("Alignment must be a power of two (1, 2, 4, 8, 16, 32, ect) : alignment: " + alignment);
					return default;
				}
				ushort x = UnsafeAs<T, ushort> (x_);
				string str = "Alignment would overflow: x: " + x + ", alignment: " + alignment;
				if (Signed<T> ()) {
					if ((short)alignment < 0) {
						Fail ("Alignment cannot be negative");
						return default;
					}

					if ((short)x >= 0) {
						if ((short)x > (short.MaxValue - (short)alignment)) {
							Fail (str);
							return default;
						}
					}

					short a = (short)((short)alignment - 1);
					return UnsafeAs<ushort, T> ((ushort)(((short)x + a) & ~a));
				} else {
					if (x >= 0) {
						if (x > (ushort.MaxValue - alignment)) {
							Fail (str);
							return default;
						}
					}

					ushort a = (ushort)(alignment - 1);
					return UnsafeAs<ushort, T> ((ushort)((x + a) & ~a));
				}
			} else if (sizeof (T) == 4 && typeof (T) != typeof (float)) {
				uint alignment = UnsafeAs<T, uint> (alignment_);
				if (alignment == 0) {
					Fail ("Alignment must not be zero");
					return default;
				}
				if ((alignment & (alignment - 1)) != 0) {
					Fail ("Alignment must be a power of two (1, 2, 4, 8, 16, 32, ect) : alignment: " + alignment);
					return default;
				}
				uint x = UnsafeAs<T, uint> (x_);
				string str = "Alignment would overflow: x: " + x + ", alignment: " + alignment;
				if (Signed<T> ()) {
					if ((int)alignment < 0) {
						Fail ("Alignment cannot be negative");
						return default;
					}

					if ((int)x >= 0) {
						if ((int)x > (int.MaxValue - (int)alignment)) {
							Fail (str);
							return default;
						}
					}

					int a = (int)((int)alignment - 1);
					return UnsafeAs<uint, T> ((uint)(((int)x + a) & ~a));
				} else {
					if (x >= 0) {
						if (x > (uint.MaxValue - alignment)) {
							Fail (str);
							return default;
						}
					}

					uint a = (uint)(alignment - 1);
					return UnsafeAs<uint, T> ((uint)((x + a) & ~a));
				}
			} else if (sizeof (T) == 8 && typeof (T) != typeof (double)) {
				ulong alignment = UnsafeAs<T, ulong> (alignment_);
				if (alignment == 0) {
					Fail ("Alignment must not be zero");
					return default;
				}
				if ((alignment & (alignment - 1)) != 0) {
					Fail ("Alignment must be a power of two (1, 2, 4, 8, 16, 32, ect) : alignment: " + alignment);
					return default;
				}
				ulong x = UnsafeAs<T, ulong> (x_);
				string str = "Alignment would overflow: x: " + x + ", alignment: " + alignment;
				if (Signed<T> ()) {
					if ((long)alignment < 0) {
						Fail ("Alignment cannot be negative");
						return default;
					}

					if ((long)x >= 0) {
						if ((long)x > (long.MaxValue - (long)alignment)) {
							Fail (str);
							return default;
						}
					}

					long a = (long)((long)alignment - 1);
					return UnsafeAs<ulong, T> ((ulong)(((long)x + a) & ~a));
				} else {
					if (x >= 0) {
						if (x > (ulong.MaxValue - alignment)) {
							Fail (str);
							return default;
						}
					}

					ulong a = (ulong)(alignment - 1);
					return UnsafeAs<ulong, T> ((ulong)((x + a) & ~a));
				}
			} else {
				return Throw<T> ();
			}
		}

		public static T Align4<T> (T x) where T : unmanaged
		{
			SKSafeMath safe = new SKSafeMath ();

			object? alignObj = Convert.ChangeType (4, typeof (T));

			if (alignObj == null) {
				safe.Fail ("could not convert 4 (" + typeof (int).FullName + ") to T (" + typeof (T).FullName + ")");
			}

			T alignment = (T)alignObj;

			return safe.AlignUp<T> (x, alignment);
		}

		public static T Align8<T> (T x) where T : unmanaged
		{
			SKSafeMath safe = new SKSafeMath ();

			object? alignObj = Convert.ChangeType (8, typeof (T));

			if (alignObj == null) {
				safe.Fail ("could not convert 4 (" + typeof (int).FullName + ") to T (" + typeof (T).FullName + ")");
			}

			T alignment = (T)alignObj;

			return safe.AlignUp<T> (x, alignment);
		}

		public enum CompareValue
		{
			INVALID_TYPE, LESS, SAME, GREATER
		}

		/// <summary>
		/// compares a and b in a generic way, only accepts integer types
		/// </summary>
		/// <returns>
		/// CompareValue.INVALID_TYPE if either of the given values are not an integer type
		/// <br></br> CompareValue.SAME if a and b are equal
		/// <br></br> CompareValue.GREATER if a is greater than b
		/// <br></br> CompareValue.LESS if a is less than b
		/// </returns>
		public unsafe static CompareValue Compare<T1, T2> (T1 a, T2 b)
			where T1 : unmanaged
			where T2 : unmanaged
		{
			if (typeof (T1) == typeof (nint) || typeof (T1) == typeof (nuint)) {
				if (typeof (T2) == typeof (nint) || typeof (T2) == typeof (nuint)) {
					nuint v1 = *(nuint*)&a;
					nuint v2 = *(nuint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (sbyte) || typeof (T2) == typeof (byte)) {
					byte v1 = (byte)*(nuint*)&a;
					byte v2 = *(byte*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (short) || typeof (T2) == typeof (ushort)) {
					ushort v1 = (ushort)*(nuint*)&a;
					ushort v2 = *(ushort*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (int) || typeof (T2) == typeof (uint)) {
					uint v1 = (uint)*(nuint*)&a;
					uint v2 = *(uint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (long) || typeof (T2) == typeof (ulong)) {
					ulong v1 = (ulong)*(nuint*)&a;
					ulong v2 = *(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				}
			} else if (typeof (T1) == typeof (sbyte) || typeof (T1) == typeof (byte)) {
				if (typeof (T2) == typeof (nint) || typeof (T2) == typeof (nuint)) {
					byte v1 = *(byte*)&a;
					byte v2 = (byte)*(nuint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (sbyte) || typeof (T2) == typeof (byte)) {
					byte v1 = *(byte*)&a;
					byte v2 = *(byte*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (short) || typeof (T2) == typeof (ushort)) {
					ushort v1 = (ushort)*(byte*)&a;
					ushort v2 = *(ushort*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (int) || typeof (T2) == typeof (uint)) {
					uint v1 = (uint)*(byte*)&a;
					uint v2 = *(uint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (long) || typeof (T2) == typeof (ulong)) {
					ulong v1 = (ulong)*(byte*)&a;
					ulong v2 = *(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				}
			} else if (typeof (T1) == typeof (short) || typeof (T1) == typeof (ushort)) {
				if (typeof (T2) == typeof (nint) || typeof (T2) == typeof (nuint)) {
					ushort v1 = *(ushort*)&a;
					ushort v2 = (ushort)*(nuint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (sbyte) || typeof (T2) == typeof (byte)) {
					ushort v1 = *(ushort*)&a;
					ushort v2 = (ushort)*(byte*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (short) || typeof (T2) == typeof (ushort)) {
					ushort v1 = *(ushort*)&a;
					ushort v2 = *(ushort*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (int) || typeof (T2) == typeof (uint)) {
					uint v1 = (uint)*(ushort*)&a;
					uint v2 = *(uint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (long) || typeof (T2) == typeof (ulong)) {
					ulong v1 = (ulong)*(ushort*)&a;
					ulong v2 = *(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				}
			} else if (typeof (T1) == typeof (int) || typeof (T1) == typeof (uint)) {
				if (typeof (T2) == typeof (nint) || typeof (T2) == typeof (nuint)) {
					uint v1 = *(uint*)&a;
					uint v2 = (uint)*(nuint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (sbyte) || typeof (T2) == typeof (byte)) {
					uint v1 = *(uint*)&a;
					uint v2 = (uint)*(byte*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (short) || typeof (T2) == typeof (ushort)) {
					uint v1 = *(uint*)&a;
					uint v2 = (uint)*(ushort*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (int) || typeof (T2) == typeof (uint)) {
					uint v1 = *(uint*)&a;
					uint v2 = *(uint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (long) || typeof (T2) == typeof (ulong)) {
					ulong v1 = (ulong)*(uint*)&a;
					ulong v2 = *(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				}
			} else if (typeof (T1) == typeof (int) || typeof (T1) == typeof (uint)) {
				if (typeof (T2) == typeof (nint) || typeof (T2) == typeof (nuint)) {
					ulong v1 = *(ulong*)&a;
					ulong v2 = (ulong)*(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (sbyte) || typeof (T2) == typeof (byte)) {
					ulong v1 = *(ulong*)&a;
					ulong v2 = (ulong)*(byte*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (short) || typeof (T2) == typeof (ushort)) {
					ulong v1 = *(ulong*)&a;
					ulong v2 = (ulong)*(ushort*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (int) || typeof (T2) == typeof (uint)) {
					ulong v1 = *(ulong*)&a;
					ulong v2 = (ulong)*(uint*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				} else if (typeof (T2) == typeof (long) || typeof (T2) == typeof (ulong)) {
					ulong v1 = *(ulong*)&a;
					ulong v2 = *(ulong*)&b;
					return v1 == v2 ? CompareValue.SAME : v1 > v2 ? CompareValue.GREATER : CompareValue.LESS;
				}
			}
			return CompareValue.INVALID_TYPE;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static T MinValue<T> () where T : unmanaged
		{
			if (typeof (T) == typeof (sbyte))
				return UnsafeAs<sbyte, T> (sbyte.MinValue);
			else if (typeof (T) == typeof (byte))
				return UnsafeAs<byte, T> (byte.MinValue);
			else if (typeof (T) == typeof (short))
				return UnsafeAs<short, T> (short.MinValue);
			else if (typeof (T) == typeof (ushort))
				return UnsafeAs<ushort, T> (ushort.MinValue);
			else if (typeof (T) == typeof (char))
				return UnsafeAs<char, T> (char.MinValue);
			else if (typeof (T) == typeof (int))
				return UnsafeAs<int, T> (int.MinValue);
			else if (typeof (T) == typeof (uint))
				return UnsafeAs<uint, T> (uint.MinValue);
			else if (typeof (T) == typeof (long))
				return UnsafeAs<long, T> (long.MinValue);
			else if (typeof (T) == typeof (ulong))
				return UnsafeAs<ulong, T> (ulong.MinValue);
			else
				return GetMinValue2 ();

			// Split remaining branches to improve chances of inlining.
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			static T GetMinValue2 ()
			{
				if (typeof (T) == typeof (nint))
					return UnsafeAs<nint, T> ((nint)(IntPtr.Size == 8 ? long.MinValue : int.MinValue));
				else if (typeof (T) == typeof (nuint))
					return UnsafeAs<nuint, T> ((nuint)(UIntPtr.Size == 8 ? ulong.MinValue : uint.MinValue));
				else
					return Throw<T> ();
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static T MaxValue<T> () where T : unmanaged
		{
			if (typeof (T) == typeof (sbyte))
				return UnsafeAs<sbyte, T> (sbyte.MaxValue);
			else if (typeof (T) == typeof (byte))
				return UnsafeAs<byte, T> (byte.MaxValue);
			else if (typeof (T) == typeof (short))
				return UnsafeAs<short, T> (short.MaxValue);
			else if (typeof (T) == typeof (ushort))
				return UnsafeAs<ushort, T> (ushort.MaxValue);
			else if (typeof (T) == typeof (char))
				return UnsafeAs<char, T> (char.MaxValue);
			else if (typeof (T) == typeof (int))
				return UnsafeAs<int, T> (int.MaxValue);
			else if (typeof (T) == typeof (uint))
				return UnsafeAs<uint, T> (uint.MaxValue);
			else if (typeof (T) == typeof (long))
				return UnsafeAs<long, T> (long.MaxValue);
			else if (typeof (T) == typeof (ulong))
				return UnsafeAs<ulong, T> (ulong.MaxValue);
			else
				return GetMaxValue2 ();

			// Split remaining branches to improve chances of inlining.
			[MethodImpl (MethodImplOptions.AggressiveInlining)]
			static T GetMaxValue2 ()
			{
				if (typeof (T) == typeof (nint))
					return UnsafeAs<nint, T> ((nint)(IntPtr.Size == 8 ? long.MaxValue : int.MaxValue));
				else if (typeof (T) == typeof (nuint))
					return UnsafeAs<nuint, T> ((nuint)(UIntPtr.Size == 8 ? ulong.MaxValue : uint.MaxValue));
				else
					return Throw<T> ();
			}
		}

		private static unsafe bool TryCastTo<T, R> (T value, out R outValue, bool log)
			where T : unmanaged
			where R : unmanaged
		{
			R min = MinValue<R> ();
			R max = MaxValue<R> ();

			CompareValue compareValueMin = Compare (value, min);
			if (compareValueMin == CompareValue.INVALID_TYPE) {
				if (log)
					LOG ("Cast from T (" + typeof (T).FullName + ") to R (" + typeof (R).FullName + ") is invalid, R must be an integer type");
				outValue = default (R);
				return false;
			}

			if (compareValueMin == CompareValue.LESS)
				goto fail;


			if (Compare (value, MaxValue<R> ()) == CompareValue.GREATER)
				goto fail;

			outValue = *(R*)&value;
			return true;

		fail:
			if (log)
				LOG ("Cast from T (" + typeof (T).FullName + ") to R (" + typeof (R).FullName + ") is invalid, the given value does not fit within the range of R.MinValue (" + min + ") and R.MaxValue (" + max + ")");
			outValue = default (R);
			return false;
		}

		public static unsafe bool TryCastTo<T, R> (T value, out R outValue)
			where T : unmanaged
			where R : unmanaged
			=> TryCastTo (value, out outValue, true);

		public unsafe R CastTo<T, R> (T value)
			where T : unmanaged
			where R : unmanaged
		{
			if (!fOK)
				return default (R);
			R r;
			if (!TryCastTo (value, out r, true)) {
				Fail ("Cast from " + typeof (T).FullName + " to " + typeof (R).FullName + " would overflow");
			}
			return r;
		}
	}
}
