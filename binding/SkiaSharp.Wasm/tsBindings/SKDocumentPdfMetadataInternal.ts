/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKDocumentPdfMetadataInternal
	{
		/* Pack=4 */
		Title : number;
		Author : number;
		Subject : number;
		Keywords : number;
		Creator : number;
		Producer : number;
		Creation : number;
		Modified : number;
		RasterDPI : number;
		PDFA : number;
		EncodingQuality : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKDocumentPdfMetadataInternal
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKDocumentPdfMetadataInternal();
			
			{
				ret.Title = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.Author = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.Subject = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.Keywords = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.Creator = Number(memoryContext.getValue(pData + 16, "*"));
			}
			
			{
				ret.Producer = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.Creation = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.Modified = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.RasterDPI = Number(memoryContext.getValue(pData + 32, "float"));
			}
			
			{
				ret.PDFA = Number(memoryContext.getValue(pData + 36, "i8"));
			}
			
			{
				ret.EncodingQuality = Number(memoryContext.getValue(pData + 40, "i32"));
			}
			return ret;
		}
		public constructor()
		{
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
			memoryContext.setValue(pData + 0, this.Title, "*");
			memoryContext.setValue(pData + 4, this.Author, "*");
			memoryContext.setValue(pData + 8, this.Subject, "*");
			memoryContext.setValue(pData + 12, this.Keywords, "*");
			memoryContext.setValue(pData + 16, this.Creator, "*");
			memoryContext.setValue(pData + 20, this.Producer, "*");
			memoryContext.setValue(pData + 24, this.Creation, "*");
			memoryContext.setValue(pData + 28, this.Modified, "*");
			memoryContext.setValue(pData + 32, this.RasterDPI, "float");
			memoryContext.setValue(pData + 36, this.PDFA, "i8");
			memoryContext.setValue(pData + 40, this.EncodingQuality, "i32");
		}
	}
}
