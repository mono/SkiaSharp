/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKWebpEncoderOptions
	{
		/* Pack=4 */
		fCompression : number;
		fQuality : number;
		fUnpremulBehavior : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKWebpEncoderOptions
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKWebpEncoderOptions();
			
			{
				ret.fCompression = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.fQuality = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.fUnpremulBehavior = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(12);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.fCompression, "i32");
			memoryContext.setValue(pData + 4, this.fQuality, "float");
			memoryContext.setValue(pData + 8, this.fUnpremulBehavior, "i32");
		}
	}
}
