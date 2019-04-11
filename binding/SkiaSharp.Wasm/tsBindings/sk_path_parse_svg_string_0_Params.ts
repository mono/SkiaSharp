/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_parse_svg_string_0_Params
	{
		/* Pack=4 */
		cpath : number;
		str : string;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_parse_svg_string_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_parse_svg_string_0_Params();
			
			{
				ret.cpath = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				var ptr = memoryContext.getValue(pData + 4, "*");
				if(ptr !== 0)
				{
					ret.str = String(memoryContext.UTF8ToString(ptr));
				}
				else
				
				{
					ret.str = null;
				}
			}
			return ret;
		}
	}
}
