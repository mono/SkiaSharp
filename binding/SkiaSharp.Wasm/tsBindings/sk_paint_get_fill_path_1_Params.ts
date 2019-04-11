/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_paint_get_fill_path_1_Params
	{
		/* Pack=4 */
		paint : number;
		src : number;
		dst : number;
		cullRectZero : number;
		resScale : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_paint_get_fill_path_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_paint_get_fill_path_1_Params();
			
			{
				ret.paint = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.dst = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.cullRectZero = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.resScale = Number(memoryContext.getValue(pData + 16, "float"));
			}
			return ret;
		}
	}
}
