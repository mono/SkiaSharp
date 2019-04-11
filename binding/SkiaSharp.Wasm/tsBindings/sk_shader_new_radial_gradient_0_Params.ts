/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_radial_gradient_0_Params
	{
		/* Pack=4 */
		center : SkiaSharp.SKPoint;
		radius : number;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		colorPos_Length : number;
		colorPos : Array<number>;
		count : number;
		mode : number;
		matrix : SkiaSharp.SKMatrix;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_radial_gradient_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_radial_gradient_0_Params();
			
			{
				ret.center = SkiaSharp.SKPoint.unmarshal(pData + 0);
			}
			
			{
				ret.radius = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*"); /*SkiaSharp.SKColor 4 False*/
				if(pArray !== 0)
				{
					ret.colors = new Array<SkiaSharp.SKColor>();
					for(var i=0; i<ret.colors_Length; i++)
					{
						ret.colors.push(SkiaSharp.SKColor.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.colors = null;
				}
			}
			
			{
				ret.colorPos_Length = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 24, "*"); /*float 4 False*/
				if(pArray !== 0)
				{
					ret.colorPos = new Array<number>();
					for(var i=0; i<ret.colorPos_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.colorPos.push(Number(value));
					}
				}
				else
				
				{
					ret.colorPos = null;
				}
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			
			{
				ret.matrix = SkiaSharp.SKMatrix.unmarshal(pData + 36);
			}
			return ret;
		}
	}
}
