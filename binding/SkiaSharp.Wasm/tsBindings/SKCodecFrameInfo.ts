/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKCodecFrameInfo
	{
		/* Pack=4 */
		requiredFrame : number;
		duration : number;
		fullyRecieved : number;
		alphaType : number;
		disposalMethod : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKCodecFrameInfo
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKCodecFrameInfo();
			
			{
				ret.requiredFrame = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.duration = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fullyRecieved = Number(memoryContext.getValue(pData + 8, "i8"));
			}
			
			{
				ret.alphaType = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.disposalMethod = Number(memoryContext.getValue(pData + 16, "i32"));
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
			memoryContext.setValue(pData + 0, this.requiredFrame, "i32");
			memoryContext.setValue(pData + 4, this.duration, "i32");
			memoryContext.setValue(pData + 8, this.fullyRecieved, "i8");
			memoryContext.setValue(pData + 12, this.alphaType, "i32");
			memoryContext.setValue(pData + 16, this.disposalMethod, "i32");
		}
	}
}
