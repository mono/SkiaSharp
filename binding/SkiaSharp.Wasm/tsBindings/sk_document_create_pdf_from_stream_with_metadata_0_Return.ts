/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_document_create_pdf_from_stream_with_metadata_0_Return
	{
		/* Pack=4 */
		metadata : SkiaSharp.SKDocumentPdfMetadataInternal;
		public constructor()
		{
			this.metadata = new SkiaSharp.SKDocumentPdfMetadataInternal();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(44);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			this.metadata.marshal(pData + 0);
		}
	}
}
