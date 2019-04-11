/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_dash_0_Params
	{
		/* Pack=4 */
		intervals_Length : number;
		intervals : Array<number>;
		count : number;
		phase : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_dash_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_dash_0_Params();
			
			{
				ret.intervals_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.intervals = new Array<number>();
					for(var i=0; i<ret.intervals_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.intervals.push(Number(value));
					}
				}
				else
				
				{
					ret.intervals = null;
				}
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.phase = Number(memoryContext.getValue(pData + 12, "float"));
			}
			return ret;
		}
	}
}
