/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKLatticeInternal
	{
		/* Pack=4 */
		fXDivs : number;
		fYDivs : number;
		fRectTypes : number;
		fXCount : number;
		fYCount : number;
		fBounds : number;
		fColors : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKLatticeInternal
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKLatticeInternal();
			
			{
				ret.fXDivs = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.fYDivs = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.fRectTypes = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.fXCount = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				ret.fYCount = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				ret.fBounds = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.fColors = Number(memoryContext.getValue(pData + 24, "*"));
			}
			return ret;
		}
		public constructor()
		{
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
			memoryContext.setValue(pData + 0, this.fXDivs, "*");
			memoryContext.setValue(pData + 4, this.fYDivs, "*");
			memoryContext.setValue(pData + 8, this.fRectTypes, "*");
			memoryContext.setValue(pData + 12, this.fXCount, "i32");
			memoryContext.setValue(pData + 16, this.fYCount, "i32");
			memoryContext.setValue(pData + 20, this.fBounds, "*");
			memoryContext.setValue(pData + 24, this.fColors, "*");
		}
	}
}
