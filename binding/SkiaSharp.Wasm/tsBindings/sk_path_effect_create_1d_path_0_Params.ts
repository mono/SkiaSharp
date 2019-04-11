/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_1d_path_0_Params
	{
		/* Pack=4 */
		path : number;
		advance : number;
		phase : number;
		style : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_1d_path_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_1d_path_0_Params();
			
			{
				ret.path = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.advance = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.phase = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.style = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
