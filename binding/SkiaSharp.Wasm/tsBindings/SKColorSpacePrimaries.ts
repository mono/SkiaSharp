/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class SKColorSpacePrimaries
	{
		/* Pack=4 */
		fRX : number;
		fRY : number;
		fGX : number;
		fGY : number;
		fBX : number;
		fBY : number;
		fWX : number;
		fWY : number;
		public static unmarshal(pData:number, memoryContext: any = null) : SKColorSpacePrimaries
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new SKColorSpacePrimaries();
			
			{
				ret.fRX = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.fRY = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.fGX = Number(memoryContext.getValue(pData + 8, "float"));
			}
			
			{
				ret.fGY = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.fBX = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.fBY = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.fWX = Number(memoryContext.getValue(pData + 24, "float"));
			}
			
			{
				ret.fWY = Number(memoryContext.getValue(pData + 28, "float"));
			}
			return ret;
		}
		public constructor()
		{
		}
		public marshalNew(memoryContext: any = null) : number
		{
			memoryContext = memoryContext ? memoryContext : Module;
			var pTarget = memoryContext._malloc(32);
			this.marshal(pTarget, memoryContext);
			return pTarget;
		}
		public marshal(pData:number, memoryContext: any = null)
		{
			memoryContext = memoryContext ? memoryContext : Module;
			memoryContext.setValue(pData + 0, this.fRX, "float");
			memoryContext.setValue(pData + 4, this.fRY, "float");
			memoryContext.setValue(pData + 8, this.fGX, "float");
			memoryContext.setValue(pData + 12, this.fGY, "float");
			memoryContext.setValue(pData + 16, this.fBX, "float");
			memoryContext.setValue(pData + 20, this.fBY, "float");
			memoryContext.setValue(pData + 24, this.fWX, "float");
			memoryContext.setValue(pData + 28, this.fWY, "float");
		}
	}
}
