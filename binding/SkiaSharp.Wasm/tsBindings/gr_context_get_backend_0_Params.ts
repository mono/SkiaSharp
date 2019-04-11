/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_context_get_backend_0_Params
	{
		/* Pack=4 */
		context : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_context_get_backend_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_context_get_backend_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
