using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_memory_mode_t
	public enum MemoryMode {
		// HB_MEMORY_MODE_DUPLICATE = 0
		Duplicate = 0,
		// HB_MEMORY_MODE_READONLY = 1
		ReadOnly = 1,
		// HB_MEMORY_MODE_WRITABLE = 2
		Writeable = 2,
		// HB_MEMORY_MODE_READONLY_MAY_MAKE_WRITABLE = 3
		ReadOnlyMayMakeWriteable = 3,
	}
}
