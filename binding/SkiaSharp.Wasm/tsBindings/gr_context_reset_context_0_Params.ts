/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_context_reset_context_0_Params
	{
		/* Pack=4 */
		context : number;
		state : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_context_reset_context_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_context_reset_context_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.state = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
	}
}
