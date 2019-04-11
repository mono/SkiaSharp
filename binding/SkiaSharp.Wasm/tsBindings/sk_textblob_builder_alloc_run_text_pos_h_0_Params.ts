/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_textblob_builder_alloc_run_text_pos_h_0_Params
	{
		/* Pack=4 */
		builder : number;
		font : number;
		count : number;
		y : number;
		textByteCount : number;
		lang : number;
		bounds : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_textblob_builder_alloc_run_text_pos_h_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_textblob_builder_alloc_run_text_pos_h_0_Params();
			
			{
				ret.builder = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.font = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.count = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.textByteCount = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.lang = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.bounds = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
	}
}
