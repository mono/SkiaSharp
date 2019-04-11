/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKCodecOptionsInternal
	{
		/* Pack=4 */
		fZeroInitialized : number;
		fSubset : number;
		fFrameIndex : number;
		fPriorFrame : number;
		fPremulBehavior : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKCodecOptionsInternal
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKCodecOptionsInternal();
			
			{
				ret.fZeroInitialized = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.fSubset = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.fFrameIndex = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.fPriorFrame = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.fPremulBehavior = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(20);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.fZeroInitialized, "i32");
			memoryContext.setValue(pData + 4, this.fSubset, "*");
			memoryContext.setValue(pData + 8, this.fFrameIndex, "i32");
			memoryContext.setValue(pData + 12, this.fPriorFrame, "i32");
			memoryContext.setValue(pData + 16, this.fPremulBehavior, "i32");
		}
	}
}
