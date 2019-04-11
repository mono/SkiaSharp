/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKJpegEncoderOptions
	{
		/* Pack=4 */
		fQuality : number;
		fDownsample : number;
		fAlphaOption : number;
		fBlendBehavior : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKJpegEncoderOptions
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKJpegEncoderOptions();
			
			{
				ret.fQuality = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.fDownsample = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fAlphaOption = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.fBlendBehavior = Number(memoryContext.getValue(pData + 12, "i32"));
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
			memoryContext.setValue(pData + 0, this.fQuality, "i32");
			memoryContext.setValue(pData + 4, this.fDownsample, "i32");
			memoryContext.setValue(pData + 8, this.fAlphaOption, "i32");
			memoryContext.setValue(pData + 12, this.fBlendBehavior, "i32");
		}
	}
}
