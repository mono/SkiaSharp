/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_color_unpremultiply_0_Params
	{
		/* Pack=4 */
		pmcolor : SkiaSharp.SKPMColor;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_color_unpremultiply_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_color_unpremultiply_0_Params();
			
			{
				ret.pmcolor = SkiaSharp.SKPMColor.unmarshal(pData + 0);
			}
			return ret;
		}
	}
}
