/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKMatrix
	{
		/* Pack=4 */
		scaleX : number;
		skewX : number;
		transX : number;
		skewY : number;
		scaleY : number;
		transY : number;
		persp0 : number;
		persp1 : number;
		persp2 : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKMatrix
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKMatrix();
			
			{
				ret.scaleX = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.skewX = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.transX = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.skewY = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.scaleY = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.transY = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.persp0 = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.persp1 = Number(memoryContext.getValue(pData + 28, "float"));
			}
			
			{
				ret.persp2 = Number(memoryContext.getValue(pData + 32, "float"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(36);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.scaleX, "float");
			memoryContext.setValue(pData + 4, this.skewX, "float");
			memoryContext.setValue(pData + 8, this.transX, "float");
			memoryContext.setValue(pData + 12, this.skewY, "float");
			memoryContext.setValue(pData + 16, this.scaleY, "float");
			memoryContext.setValue(pData + 20, this.transY, "float");
			memoryContext.setValue(pData + 24, this.persp0, "float");
			memoryContext.setValue(pData + 28, this.persp1, "float");
			memoryContext.setValue(pData + 32, this.persp2, "float");
		}
	}
}
