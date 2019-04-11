/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_string_get_c_str_0_Params
	{
		/* Pack=4 */
		skstring : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_string_get_c_str_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_string_get_c_str_0_Params();
			
			{
				ret.skstring = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
