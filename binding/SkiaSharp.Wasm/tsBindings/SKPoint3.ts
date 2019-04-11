/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKPoint3
	{
		/* Pack=4 */
		x : number;
		y : number;
		z : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKPoint3
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKPoint3();
			
			{
				ret.x = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.y = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.z = Number(memoryContext.getValue(pData + 8, "float"));
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
			memoryContext.setValue(pData + 0, this.x, "float");
			memoryContext.setValue(pData + 4, this.y, "float");
			memoryContext.setValue(pData + 8, this.z, "float");
		}
	}
}
