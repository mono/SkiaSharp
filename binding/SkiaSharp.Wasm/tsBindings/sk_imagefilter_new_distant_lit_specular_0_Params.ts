/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_distant_lit_specular_0_Params
	{
		/* Pack=4 */
		direction : SkiaSharp.SKPoint3;
		lightColor : SkiaSharp.SKColor;
		surfaceScale : number;
		ks : number;
		shininess : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_distant_lit_specular_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_distant_lit_specular_0_Params();
			
			{
				ret.direction = SkiaSharp.SKPoint3.unmarshal(pData + 0);
			}
			
			{
				ret.lightColor = SkiaSharp.SKColor.unmarshal(pData + 12);
			}
			
			{
				ret.surfaceScale = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.ks = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.shininess = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 32, "*"));
			}
			return ret;
		}
	}
}
