/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_document_begin_page_1_Params
	{
		/* Pack=4 */
		document : number;
		width : number;
		height : number;
		contentZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_document_begin_page_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_document_begin_page_1_Params();
			
			{
				ret.document = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.width = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.height = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.contentZero = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
