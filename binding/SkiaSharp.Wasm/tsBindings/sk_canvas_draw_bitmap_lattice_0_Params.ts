/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_draw_bitmap_lattice_0_Params
	{
		/* Pack=4 */
		t : number;
		bitmap : number;
		lattice : SkiaSharp.SKLatticeInternal;
		dst : SkiaSharp.SKRect;
		paint : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_draw_bitmap_lattice_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_draw_bitmap_lattice_0_Params();
			
			{
				ret.t = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.bitmap = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.lattice = SkiaSharp.SKLatticeInternal.unmarshal(pData + 8);
			}
			
			{
				ret.dst = SkiaSharp.SKRect.unmarshal(pData + 36);
			}
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 52, "*"));
			}
			return ret;
		}
	}
}
