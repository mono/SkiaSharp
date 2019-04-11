/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKMask
	{
		/* Pack=4 */
		image : number;
		bounds : SkiaSharp.SKRectI;
		rowBytes : number;
		format : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKMask
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKMask();
			
			{
				ret.image = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.bounds = SkiaSharp.SKRectI.unmarshal(pData + 4);
			}
			
			{
				ret.rowBytes = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				ret.format = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			return ret;
		}
		public constructor()
		{
			this.bounds = new SkiaSharp.SKRectI();
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(28);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.image, "*");
			this.bounds.marshal(pData + 4);
			memoryContext.setValue(pData + 20, this.rowBytes, "i32");
			memoryContext.setValue(pData + 24, this.format, "i32");
		}
	}
}
