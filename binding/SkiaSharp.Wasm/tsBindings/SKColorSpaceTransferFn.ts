/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKColorSpaceTransferFn
	{
		/* Pack=4 */
		fG : number;
		fA : number;
		fB : number;
		fC : number;
		fD : number;
		fE : number;
		fF : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKColorSpaceTransferFn
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKColorSpaceTransferFn();
			
			{
				ret.fG = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.fA = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.fB = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.fC = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.fD = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.fE = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.fF = Number(memoryContext.getValue(pData + 24, "float"));
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
			memoryContext.setValue(pData + 0, this.fG, "float");
			memoryContext.setValue(pData + 4, this.fA, "float");
			memoryContext.setValue(pData + 8, this.fB, "float");
			memoryContext.setValue(pData + 12, this.fC, "float");
			memoryContext.setValue(pData + 16, this.fD, "float");
			memoryContext.setValue(pData + 20, this.fE, "float");
			memoryContext.setValue(pData + 24, this.fF, "float");
		}
	}
}
