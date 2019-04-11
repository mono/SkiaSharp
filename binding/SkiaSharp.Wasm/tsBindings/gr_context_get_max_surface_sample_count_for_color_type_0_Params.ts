/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_context_get_max_surface_sample_count_for_color_type_0_Params
	{
		/* Pack=4 */
		context : number;
		colorType : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_context_get_max_surface_sample_count_for_color_type_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_context_get_max_surface_sample_count_for_color_type_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.colorType = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
