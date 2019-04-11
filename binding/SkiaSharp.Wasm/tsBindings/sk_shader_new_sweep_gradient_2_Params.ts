/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_sweep_gradient_2_Params
	{
		/* Pack=4 */
		center : SkiaSharp.SKPoint;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		colorPosZero : number;
		count : number;
		mode : number;
		startAngle : number;
		endAngle : number;
		matrixZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_sweep_gradient_2_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_sweep_gradient_2_Params();
			
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
				ret.colorPosZero = Number(memoryContext.getValue(pData + 16, "*"));
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			
			{
				ret.startAngle = Number(memoryContext.getValue(pData + 28, "float"));
			}
			
			{
				ret.endAngle = Number(memoryContext.getValue(pData + 32, "float"));
			}
			
			{
				ret.matrixZero = Number(memoryContext.getValue(pData + 36, "*"));
			}
			return ret;
		}
	}
}
