/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_glinterface_assemble_interface_0_Params
	{
		/* Pack=4 */
		ctx : number;
		get : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_glinterface_assemble_interface_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_glinterface_assemble_interface_0_Params();
			
			{
				ret.ctx = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.get = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
