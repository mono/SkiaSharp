/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_context_set_resource_cache_limits_0_Params
	{
		/* Pack=4 */
		context : number;
		maxResources : number;
		maxResourceBytes : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_context_set_resource_cache_limits_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_context_set_resource_cache_limits_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.maxResources = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.maxResourceBytes = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
