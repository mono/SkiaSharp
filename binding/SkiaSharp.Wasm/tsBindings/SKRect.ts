/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKRect
	{
		/* Pack=4 */
		left : number;
		top : number;
		right : number;
		bottom : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKRect
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKRect();
			
			{
				ret.left = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.top = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.right = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.bottom = Number(memoryContext.getValue(pData + 12, "float"));
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
			memoryContext.setValue(pData + 0, this.left, "float");
			memoryContext.setValue(pData + 4, this.top, "float");
			memoryContext.setValue(pData + 8, this.right, "float");
			memoryContext.setValue(pData + 12, this.bottom, "float");
		}
	}
}
