/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_iter_next_0_Params
	{
		/* Pack=4 */
		iterator : number;
		points_Length : number;
		points : Array<SkiaSharp.SKPoint>;
		doConsumeDegenerates : number;
		exact : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_iter_next_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_iter_next_0_Params();
			
			{
				ret.iterator = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.points_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
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
				ret.doConsumeDegenerates = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.exact = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
	}
}
