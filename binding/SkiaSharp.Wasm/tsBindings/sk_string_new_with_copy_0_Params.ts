/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_string_new_with_copy_0_Params
	{
		/* Pack=4 */
		src_Length : number;
		src : Array<number>;
		length : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_string_new_with_copy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_string_new_with_copy_0_Params();
			
			{
				ret.src_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*"); /*byte 1 False*/
				if(pArray !== 0)
				{
					ret.src = new Array<number>();
					for(var i=0; i<ret.src_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 1, "i8");
						ret.src.push(Number(value));
					}
				}
				else
				
				{
					ret.src = null;
				}
			}
			
			{
				ret.length = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
