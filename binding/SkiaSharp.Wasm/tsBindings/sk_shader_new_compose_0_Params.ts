/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_compose_0_Params
	{
		/* Pack=4 */
		shaderA : number;
		shaderB : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_compose_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_compose_0_Params();
			
			{
				ret.shaderA = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.shaderB = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
