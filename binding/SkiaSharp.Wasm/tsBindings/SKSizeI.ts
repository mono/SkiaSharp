/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKSizeI
	{
		/* Pack=4 */
		width : number;
		height : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKSizeI
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKSizeI();
			
			{
				ret.width = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.height = Number(memoryContext.getValue(pData + 4, "i32"));
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
			memoryContext.setValue(pData + 0, this.width, "i32");
			memoryContext.setValue(pData + 4, this.height, "i32");
		}
	}
}
