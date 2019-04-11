/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_get_table_data_0_Params
	{
		/* Pack=4 */
		typeface : number;
		tag : number;
		offset : number;
		length : number;
		data_Length : number;
		data : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_get_table_data_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_get_table_data_0_Params();
			
			{
				ret.typeface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.tag = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.offset = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.data_Length = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 20, "*"); /*byte 1 False*/
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
			return ret;
		}
	}
}
