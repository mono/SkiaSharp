/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_get_table_tags_0_Params
	{
		/* Pack=4 */
		typeface : number;
		tags_Length : number;
		tags : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_get_table_tags_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_get_table_tags_0_Params();
			
			{
				ret.typeface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.tags_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
				if(pArray !== 0)
				{
					ret.tags = new Array<number>();
					for(var i=0; i<ret.tags_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i32");
						ret.tags.push(Number(value));
					}
				}
				else
				
				{
					ret.tags = null;
				}
			}
			return ret;
		}
	}
}
