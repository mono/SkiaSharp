/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_document_end_page_0_Params
	{
		/* Pack=4 */
		document : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_document_end_page_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_document_end_page_0_Params();
			
			{
				ret.document = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
