using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_cluster_level_t
	/// <summary>The various levels of buffer clustering.</summary>
	/// <remarks />
	public enum ClusterLevel {
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES = 0
		/// <summary>Cluster values grouped by graphemes into monotone order.</summary>
		MonotoneGraphemes = 0,
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_CHARACTERS = 1
		/// <summary>Cluster values grouped into monotone order.</summary>
		MonotoneCharacters = 1,
		// HB_BUFFER_CLUSTER_LEVEL_CHARACTERS = 2
		/// <summary>Don't group cluster values.</summary>
		Characters = 2,
		// HB_BUFFER_CLUSTER_LEVEL_GRAPHEMES = 3
		/// <summary>Cluster values grouped by graphemes without requiring monotone order.</summary>
		Graphemes = 3,
		// HB_BUFFER_CLUSTER_LEVEL_DEFAULT = HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES
		/// <summary>Default cluster level (<see cref="F:HarfBuzzSharp.ClusterLevel.MonotoneGraphemes" />).</summary>
		Default = 0,
	}
}
