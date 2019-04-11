/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_color_matrix_0_Params
	{
		/* Pack=4 */
		array_Length : number;
		array : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_color_matrix_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_color_matrix_0_Params();
			
			{
				ret.array_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.array = new Array<number>();
					for(var i=0; i<ret.array_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.array.push(Number(value));
					}
				}
				else
				
				{
					ret.array = null;
				}
			}
			return ret;
		}
	}
}
