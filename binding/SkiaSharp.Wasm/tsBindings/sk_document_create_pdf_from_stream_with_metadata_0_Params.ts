/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_document_create_pdf_from_stream_with_metadata_0_Params
	{
		/* Pack=4 */
		stream : number;
		metadata : SkiaSharp.SKDocumentPdfMetadataInternal;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_document_create_pdf_from_stream_with_metadata_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_document_create_pdf_from_stream_with_metadata_0_Params();
			
			{
				ret.stream = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.metadata = SkiaSharp.SKDocumentPdfMetadataInternal.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
