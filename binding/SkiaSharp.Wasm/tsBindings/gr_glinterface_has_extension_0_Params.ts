/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_glinterface_has_extension_0_Params
	{
		/* Pack=4 */
		glInterface : number;
		extension : string;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_glinterface_has_extension_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_glinterface_has_extension_0_Params();
			
			{
				ret.glInterface = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				var ptr = memoryContext.getValue(pData + 4, "*");
				if(ptr !== 0)
				{
					ret.extension = String(memoryContext.UTF8ToString(ptr));
				}
				else
				
				{
					ret.extension = null;
				}
			}
			return ret;
		}
	}
}
