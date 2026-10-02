using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_meta_tag_t
	/// <summary>Tags for the OpenType meta table.</summary>
	/// <remarks />
	public enum OpenTypeMetaTag {
		// HB_OT_META_TAG_DESIGN_LANGUAGES = 1684827751
		/// <summary>Languages the font was designed for.</summary>
		DesignLanguages = 1684827751,
		// HB_OT_META_TAG_SUPPORTED_LANGUAGES = 1936485991
		/// <summary>Languages the font supports.</summary>
		SupportedLanguages = 1936485991,
	}
}
