/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_get_points_0_Params
	{
		/* Pack=4 */
		path : number;
		points_Length : number;
		points : Array<SkiaSharp.SKPoint>;
		max : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_get_points_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_get_points_0_Params();
			
			{
				ret.path = Number(memoryContext.getValue(pData + 0, "*"));
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
				ret.max = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
