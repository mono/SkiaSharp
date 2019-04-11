/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendrendertarget_is_valid_0_Params
	{
		/* Pack=4 */
		rendertarget : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_backendrendertarget_is_valid_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_backendrendertarget_is_valid_0_Params();
			
			{
				ret.rendertarget = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
