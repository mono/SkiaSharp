/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_svgcanvas_create_0_Params
	{
		/* Pack=4 */
		bounds : SkiaSharp.SKRect;
		writer : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_svgcanvas_create_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_svgcanvas_create_0_Params();
			
			{
				ret.bounds = SkiaSharp.SKRect.unmarshal(pData + 0);
			}
			
			{
				ret.writer = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
