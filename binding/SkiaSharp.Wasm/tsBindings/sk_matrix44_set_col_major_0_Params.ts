/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_set_col_major_0_Params
	{
		/* Pack=4 */
		matrix : number;
		src_Length : number;
		src : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_set_col_major_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_set_col_major_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*"); /*float 4 False*/
				if(pArray !== 0)
				{
					ret.src = new Array<number>();
					for(var i=0; i<ret.src_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.src.push(Number(value));
					}
				}
				else
				
				{
					ret.src = null;
				}
			}
			return ret;
		}
	}
}
