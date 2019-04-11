/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_canvas_new_from_bitmap_0_Params
	{
		/* Pack=4 */
		bitmap : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_canvas_new_from_bitmap_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_canvas_new_from_bitmap_0_Params();
			
			{
				ret.bitmap = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
