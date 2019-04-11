/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_color_filter_0_Params
	{
		/* Pack=4 */
		proxy : number;
		filter : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_color_filter_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_color_filter_0_Params();
			
			{
				ret.proxy = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.filter = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
