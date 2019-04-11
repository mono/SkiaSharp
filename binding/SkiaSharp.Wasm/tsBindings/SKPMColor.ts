/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKPMColor
	{
		/* Pack=4 */
		color : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKPMColor
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKPMColor();
			
			{
				ret.color = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(4);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.color, "i32");
		}
	}
}
