/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class GRGlTextureInfo
	{
		/* Pack=4 */
		fTarget : number;
		fID : number;
		fFormat : number;
		public static unmarshal(pData:number, memoryContext: any = null) : GRGlTextureInfo
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new GRGlTextureInfo();
			
			{
				ret.fTarget = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.fID = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fFormat = Number(memoryContext.getValue(pData + 8, "i32"));
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
			memoryContext.setValue(pData + 0, this.fTarget, "i32");
			memoryContext.setValue(pData + 4, this.fID, "i32");
			memoryContext.setValue(pData + 8, this.fFormat, "i32");
		}
	}
}
