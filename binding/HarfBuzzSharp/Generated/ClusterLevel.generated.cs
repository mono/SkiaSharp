using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_cluster_level_t
	public enum ClusterLevel {
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES = 0
		MonotoneGraphemes = 0,
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_CHARACTERS = 1
		MonotoneCharacters = 1,
		// HB_BUFFER_CLUSTER_LEVEL_CHARACTERS = 2
		Characters = 2,
		// HB_BUFFER_CLUSTER_LEVEL_DEFAULT = HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES
		Default = 0,
	}
}
