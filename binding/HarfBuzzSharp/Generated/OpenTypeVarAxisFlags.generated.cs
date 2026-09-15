using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_var_axis_flags_t
	/// <summary>Specifies flags for variable font axes.</summary>
	/// <remarks />
	public enum OpenTypeVarAxisFlags {
		// HB_OT_VAR_AXIS_FLAG_HIDDEN = 0x00000001u
		/// <summary>The axis should not be exposed directly in user interfaces.</summary>
		Hidden = 1,
	}
}
