/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKPointI
	{
		/* Pack=4 */
		x : number;
		y : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKPointI
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKPointI();
			
			{
				ret.x = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(8);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.x, "i32");
			memoryContext.setValue(pData + 4, this.y, "i32");
		}
	}
}
