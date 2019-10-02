using System;

namespace HarfBuzzSharp
{
	public partial struct Script : IEquatable<Script>
	{
		private readonly Tag tag;

		private Script (Tag tag)
		{
			this.tag = tag;
		}

		public Direction HorizontalDirection =>
			HarfBuzzApi.hb_script_get_horizontal_direction (tag);

		public static Script Parse (string str) =>
			HarfBuzzApi.hb_script_from_string (str, -1);

		public override string ToString () => tag.ToString ();

		public static implicit operator uint (Script script) => script.tag;

		public static implicit operator Script (uint tag) => new Script (tag);

		public override bool Equals (object obj) =>
			obj is Script script ? tag.Equals (script.tag) : false;

		public bool Equals (Script other) => tag.Equals (other.tag);

		public override int GetHashCode () => tag.GetHashCode ();
	}
}
