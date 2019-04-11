/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_sweep_gradient_0_Params
	{
		/* Pack=4 */
		center : SkiaSharp.SKPoint;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		colorPos_Length : number;
		colorPos : Array<number>;
		count : number;
		mode : number;
		startAngle : number;
		endAngle : number;
		matrixZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_sweep_gradient_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_sweep_gradient_0_Params();
			
			{
				ret.center = SkiaSharp.SKPoint.unmarshal(pData + 0);
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 12, "*"); /*SkiaSharp.SKColor 4 False*/
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
				ret.colorPos_Length = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 20, "*"); /*float 4 False*/
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
				ret.count = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 28, "i32"));
			}
			
			{
				ret.startAngle = Number(memoryContext.getValue(pData + 32, "float"));
			}
			
			{
				ret.endAngle = Number(memoryContext.getValue(pData + 36, "float"));
			}
			
			{
				ret.matrixZero = Number(memoryContext.getValue(pData + 40, "*"));
			}
			return ret;
		}
	}
}
