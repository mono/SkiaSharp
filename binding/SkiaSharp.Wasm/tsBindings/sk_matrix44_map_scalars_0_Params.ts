/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_matrix44_map_scalars_0_Params
	{
		/* Pack=4 */
		matrix : number;
		src_Length : number;
		src : Array<number>;
		dst_Length : number;
		dst : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_matrix44_map_scalars_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_matrix44_map_scalars_0_Params();
			
			{
				ret.matrix = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.src_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
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
			
			{
				ret.dst_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*");
				if(pArray !== 0)
				{
					ret.dst = new Array<number>();
					for(var i=0; i<ret.dst_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.dst.push(Number(value));
					}
				}
				else
				
				{
					ret.dst = null;
				}
			}
			return ret;
		}
	}
}
