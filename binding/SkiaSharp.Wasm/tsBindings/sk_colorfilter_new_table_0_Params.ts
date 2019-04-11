/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_table_0_Params
	{
		/* Pack=4 */
		table_Length : number;
		table : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_table_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_table_0_Params();
			
			{
				ret.table_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.table = new Array<number>();
					for(var i=0; i<ret.table_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.table.push(Number(value));
					}
				}
				else
				
				{
					ret.table = null;
				}
			}
			return ret;
		}
	}
}
