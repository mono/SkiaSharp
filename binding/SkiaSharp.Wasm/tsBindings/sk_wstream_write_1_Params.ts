/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_wstream_write_1_Params
	{
		/* Pack=4 */
		cstream : number;
		buffer_Length : number;
		buffer : Array<number>;
		size : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_wstream_write_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_wstream_write_1_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.buffer_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
				if(pArray !== 0)
				{
					ret.buffer = new Array<number>();
					for(var i=0; i<ret.buffer_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i8");
						ret.buffer.push(Number(value));
					}
				}
				else
				
				{
					ret.buffer = null;
				}
			}
			
			{
				ret.size = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
