/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_spot_lit_specular_0_Params
	{
		/* Pack=4 */
		location : SkiaSharp.SKPoint3;
		target : SkiaSharp.SKPoint3;
		specularExponent : number;
		cutoffAngle : number;
		lightColor : SkiaSharp.SKColor;
		surfaceScale : number;
		ks : number;
		shininess : number;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_spot_lit_specular_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_spot_lit_specular_0_Params();
			
			{
				ret.location = SkiaSharp.SKPoint3.unmarshal(pData + 0);
			}
			
			{
				ret.target = SkiaSharp.SKPoint3.unmarshal(pData + 12);
			}
			
			{
				ret.specularExponent = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.cutoffAngle = Number(memoryContext.getValue(pData + 28, "float"));
			}
			
			{
				ret.lightColor = SkiaSharp.SKColor.unmarshal(pData + 32);
			}
			
			{
				ret.surfaceScale = Number(memoryContext.getValue(pData + 36, "float"));
			}
			
			{
				ret.ks = Number(memoryContext.getValue(pData + 40, "float"));
			}
			
			{
				ret.shininess = Number(memoryContext.getValue(pData + 44, "float"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 48, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 52, "*"));
			}
			return ret;
		}
	}
}
