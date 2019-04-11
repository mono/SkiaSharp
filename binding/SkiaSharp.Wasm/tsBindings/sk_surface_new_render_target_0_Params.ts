/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_new_render_target_0_Params
	{
		/* Pack=4 */
		context : number;
		budgeted : boolean;
		info : SkiaSharp.SKImageInfoNative;
		sampleCount : number;
		origin : number;
		props : number;
		shouldCreateWithMips : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_new_render_target_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_new_render_target_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.budgeted = Boolean(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.info = SkiaSharp.SKImageInfoNative.unmarshal(pData + 8);
			}
			
			{
				ret.sampleCount = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			
			{
				ret.origin = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			
			{
				ret.props = Number(memoryContext.getValue(pData + 36, "*"));
			}
			
			{
				ret.shouldCreateWithMips = Boolean(memoryContext.getValue(pData + 40, "i32"));
			}
			return ret;
		}
	}
}
