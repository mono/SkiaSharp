/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_document_create_xps_from_stream_0_Params
	{
		/* Pack=4 */
		stream : number;
		dpi : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_document_create_xps_from_stream_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_document_create_xps_from_stream_0_Params();
			
			{
				ret.stream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.dpi = Number(memoryContext.getValue(pData + 4, "float"));
			}
			return ret;
		}
	}
}
