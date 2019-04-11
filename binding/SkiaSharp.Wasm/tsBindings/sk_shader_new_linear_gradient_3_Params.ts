/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_linear_gradient_3_Params
	{
		/* Pack=4 */
		points_Length : number;
		points : Array<SkiaSharp.SKPoint>;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		colorPosZero : number;
		count : number;
		mode : number;
		matrixZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_linear_gradient_3_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_linear_gradient_3_Params();
			
			{
				ret.points_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.points = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.points_Length; i++)
					{
						ret.points.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.points = null;
				}
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 12, "*");
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
				ret.matrixZero = Number(memoryContext.getValue(pData + 28, "*"));
			}
			return ret;
		}
	}
}
