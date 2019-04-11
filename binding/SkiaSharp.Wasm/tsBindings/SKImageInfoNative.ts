/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKImageInfoNative
	{
		/* Pack=4 */
		fColorSpace : number;
		fWidth : number;
		fHeight : number;
		fColorType : number;
		fAlphaType : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKImageInfoNative
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKImageInfoNative();
			
			{
				ret.fColorSpace = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.fWidth = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fHeight = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.fColorType = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.fAlphaType = Number(memoryContext.getValue(pData + 16, "i32"));
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
			memoryContext.setValue(pData + 0, this.fColorSpace, "*");
			memoryContext.setValue(pData + 4, this.fWidth, "i32");
			memoryContext.setValue(pData + 8, this.fHeight, "i32");
			memoryContext.setValue(pData + 12, this.fColorType, "i32");
			memoryContext.setValue(pData + 16, this.fAlphaType, "i32");
		}
	}
}
