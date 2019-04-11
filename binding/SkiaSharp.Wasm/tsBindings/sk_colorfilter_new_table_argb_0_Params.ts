/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorfilter_new_table_argb_0_Params
	{
		/* Pack=4 */
		tableA_Length : number;
		tableA : Array<number>;
		tableR_Length : number;
		tableR : Array<number>;
		tableG_Length : number;
		tableG : Array<number>;
		tableB_Length : number;
		tableB : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorfilter_new_table_argb_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorfilter_new_table_argb_0_Params();
			
			{
				ret.tableA_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.tableA = new Array<number>();
					for(var i=0; i<ret.tableA_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.tableA.push(Number(value));
					}
				}
				else
				
				{
					ret.tableA = null;
				}
			}
			
			{
				ret.tableR_Length = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 12, "*");
				if(pArray !== 0)
				{
					ret.tableR = new Array<number>();
					for(var i=0; i<ret.tableR_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.tableR.push(Number(value));
					}
				}
				else
				
				{
					ret.tableR = null;
				}
			}
			
			{
				ret.tableG_Length = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 20, "*");
				if(pArray !== 0)
				{
					ret.tableG = new Array<number>();
					for(var i=0; i<ret.tableG_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.tableG.push(Number(value));
					}
				}
				else
				
				{
					ret.tableG = null;
				}
			}
			
			{
				ret.tableB_Length = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 28, "*");
				if(pArray !== 0)
				{
					ret.tableB = new Array<number>();
					for(var i=0; i<ret.tableB_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.tableB.push(Number(value));
					}
				}
				else
				
				{
					ret.tableB = null;
				}
			}
			return ret;
		}
	}
}
