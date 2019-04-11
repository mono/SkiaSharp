/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_add_rounded_rect_0_Params
	{
		/* Pack=4 */
		t : number;
		rect : SkiaSharp.SKRect;
		rx : number;
		ry : number;
		dir : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_add_rounded_rect_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_add_rounded_rect_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			
			{
				ret.rx = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.ry = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.dir = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			return ret;
		}
	}
}
