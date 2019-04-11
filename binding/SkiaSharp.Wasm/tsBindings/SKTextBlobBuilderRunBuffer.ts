/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKTextBlobBuilderRunBuffer
	{
		/* Pack=4 */
		glyphs : number;
		pos : number;
		utf8text : number;
		clusters : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKTextBlobBuilderRunBuffer
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKTextBlobBuilderRunBuffer();
			
			{
				ret.glyphs = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.pos = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.utf8text = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.clusters = Number(memoryContext.getValue(pData + 12, "*"));
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
			memoryContext.setValue(pData + 0, this.glyphs, "*");
			memoryContext.setValue(pData + 4, this.pos, "*");
			memoryContext.setValue(pData + 8, this.utf8text, "*");
			memoryContext.setValue(pData + 12, this.clusters, "*");
		}
	}
}
