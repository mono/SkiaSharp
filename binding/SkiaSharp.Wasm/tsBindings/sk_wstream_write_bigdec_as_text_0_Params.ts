/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_wstream_write_bigdec_as_text_0_Params
	{
		/* Pack=4 */
		cstream : number;
		value : number;
		minDigits : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_wstream_write_bigdec_as_text_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_wstream_write_bigdec_as_text_0_Params();
			
			{
				ret.cstream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.value = Number(memoryContext.getValue(pData + 4, "i64"));
			}
			
			{
				ret.minDigits = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			return ret;
		}
	}
}
