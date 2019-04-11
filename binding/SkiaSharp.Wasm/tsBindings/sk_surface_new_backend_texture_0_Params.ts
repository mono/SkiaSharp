/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_surface_new_backend_texture_0_Params
	{
		/* Pack=4 */
		context : number;
		texture : number;
		origin : number;
		samples : number;
		colorType : number;
		colorspace : number;
		props : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_surface_new_backend_texture_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_surface_new_backend_texture_0_Params();
			
			{
				ret.context = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.texture = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.origin = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.samples = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.colorType = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.colorspace = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.props = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
