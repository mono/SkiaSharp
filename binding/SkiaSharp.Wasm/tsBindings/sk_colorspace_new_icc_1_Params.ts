/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_colorspace_new_icc_1_Params
	{
		/* Pack=4 */
		input_Length : number;
		input : Array<number>;
		len : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_colorspace_new_icc_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_colorspace_new_icc_1_Params();
			
			{
				ret.input_Length = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 4, "*");
				if(pArray !== 0)
				{
					ret.input = new Array<number>();
					for(var i=0; i<ret.input_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.input.push(Number(value));
					}
				}
				else
				
				{
					ret.input = null;
				}
			}
			
			{
				ret.len = Number(memoryContext.getValue(pData + 8, "*"));
			}
			return ret;
		}
	}
}
