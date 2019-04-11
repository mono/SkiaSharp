/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_managedstream_set_delegates_0_Params
	{
		/* Pack=4 */
		pRead : number;
		pPeek : number;
		pIsAtEnd : number;
		pHasPosition : number;
		pHasLength : number;
		pRewind : number;
		pGetPosition : number;
		pSeek : number;
		pMove : number;
		pGetLength : number;
		pCreateNew : number;
		pDestroy : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_managedstream_set_delegates_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_managedstream_set_delegates_0_Params();
			
			{
				ret.pRead = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.pPeek = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.pIsAtEnd = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.pHasPosition = Number(memoryContext.getValue(pData + 12, "*"));
			}
			
			{
				ret.pHasLength = Number(memoryContext.getValue(pData + 16, "*"));
			}
			
			{
				ret.pRewind = Number(memoryContext.getValue(pData + 20, "*"));
			}
			
			{
				ret.pGetPosition = Number(memoryContext.getValue(pData + 24, "*"));
			}
			
			{
				ret.pSeek = Number(memoryContext.getValue(pData + 28, "*"));
			}
			
			{
				ret.pMove = Number(memoryContext.getValue(pData + 32, "*"));
			}
			
			{
				ret.pGetLength = Number(memoryContext.getValue(pData + 36, "*"));
			}
			
			{
				ret.pCreateNew = Number(memoryContext.getValue(pData + 40, "*"));
			}
			
			{
				ret.pDestroy = Number(memoryContext.getValue(pData + 44, "*"));
			}
			return ret;
		}
	}
}
