/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKRectI
	{
		/* Pack=4 */
		left : number;
		top : number;
		right : number;
		bottom : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKRectI
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKRectI();
			
			{
				ret.left = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.top = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.right = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.bottom = Number(memoryContext.getValue(pData + 12, "i32"));
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
			memoryContext.setValue(pData + 0, this.left, "i32");
			memoryContext.setValue(pData + 4, this.top, "i32");
			memoryContext.setValue(pData + 8, this.right, "i32");
			memoryContext.setValue(pData + 12, this.bottom, "i32");
		}
	}
}
