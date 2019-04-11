/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKPngEncoderOptions
	{
		/* Pack=4 */
		fFilterFlags : number;
		fZLibLevel : number;
		fUnpremulBehavior : number;
		fComments : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKPngEncoderOptions
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKPngEncoderOptions();
			
			{
				ret.fFilterFlags = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.fZLibLevel = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fUnpremulBehavior = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.fComments = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(16);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.fFilterFlags, "i32");
			memoryContext.setValue(pData + 4, this.fZLibLevel, "i32");
			memoryContext.setValue(pData + 8, this.fUnpremulBehavior, "i32");
			memoryContext.setValue(pData + 12, this.fComments, "*");
		}
	}
}
