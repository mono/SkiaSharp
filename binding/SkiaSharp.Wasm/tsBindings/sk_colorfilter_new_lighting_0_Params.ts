/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_lighting_0_Params
	{
		/* Pack=4 */
		mul : SkiaSharp.SKColor;
		add : SkiaSharp.SKColor;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_lighting_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_lighting_0_Params();
			
			{
				ret.mul = SkiaSharp.SKColor.unmarshal(pData + 0);
			}
			
			{
				ret.add = SkiaSharp.SKColor.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
