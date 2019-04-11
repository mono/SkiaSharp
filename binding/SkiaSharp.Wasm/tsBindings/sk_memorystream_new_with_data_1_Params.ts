/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_memorystream_new_with_data_1_Params
	{
		/* Pack=4 */
		data_Length : number;
		data : Array<number>;
		length : number;
		copyData : boolean;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_memorystream_new_with_data_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_memorystream_new_with_data_1_Params();
			
			{
				ret.data_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*"); /*byte 1 False*/
				if(pArray !== 0)
				{
					ret.data = new Array<number>();
					for(var i=0; i<ret.data_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 1, "i8");
						ret.data.push(Number(value));
					}
				}
				else
				
				{
					ret.data = null;
				}
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.copyData = Boolean(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
