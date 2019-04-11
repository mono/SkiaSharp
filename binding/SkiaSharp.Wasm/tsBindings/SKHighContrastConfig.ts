/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKHighContrastConfig
	{
		/* Pack=4 */
		fGrayscale : number;
		fInvertStyle : number;
		fContrast : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKHighContrastConfig
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKHighContrastConfig();
			
			{
				ret.fGrayscale = Number(memoryContext.getValue(pData + 0, "i8"));
			}
			
			{
				ret.fInvertStyle = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.fContrast = Number(memoryContext.getValue(pData + 8, "float"));
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
			memoryContext.setValue(pData + 0, this.fGrayscale, "i8");
			memoryContext.setValue(pData + 4, this.fInvertStyle, "i32");
			memoryContext.setValue(pData + 8, this.fContrast, "float");
		}
	}
}
