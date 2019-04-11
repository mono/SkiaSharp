/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_merge_0_Params
	{
		/* Pack=4 */
		filters_Length : number;
		filters : Array<number>;
		count : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_merge_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_merge_0_Params();
			
			{
				ret.filters_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*"); /*System.IntPtr 4 False*/
				if(pArray !== 0)
				{
					ret.filters = new Array<number>();
					for(var i=0; i<ret.filters_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "*");
						ret.filters.push(Number(value));
					}
				}
				else
				
				{
					ret.filters = null;
				}
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
