/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_color_premultiply_0_Params
	{
		/* Pack=4 */
		color : SkiaSharp.SKColor;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_color_premultiply_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_color_premultiply_0_Params();
			
			{
				ret.color = SkiaSharp.SKColor.unmarshal(pData + 0);
			}
			return ret;
		}
	}
}
