/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_managedwstream_set_delegates_0_Params
	{
		/* Pack=4 */
		pWrite : number;
		pFlush : number;
		pBytesWritten : number;
		pDestroy : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_managedwstream_set_delegates_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_managedwstream_set_delegates_0_Params();
			
			{
				ret.pWrite = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.pFlush = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.pBytesWritten = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.pDestroy = Number(memoryContext.getValue(pData + 12, "*"));
			}
			return ret;
		}
	}
}
