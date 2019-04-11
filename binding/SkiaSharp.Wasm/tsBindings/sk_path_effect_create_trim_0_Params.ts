/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_trim_0_Params
	{
		/* Pack=4 */
		start : number;
		stop : number;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_trim_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_trim_0_Params();
			
			{
				ret.start = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.stop = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
